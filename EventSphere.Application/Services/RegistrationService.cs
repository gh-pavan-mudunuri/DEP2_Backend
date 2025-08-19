
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using EventSphere.Application.Dtos.Registrations;
using EventSphere.Application.Interfaces;
// using EventSphere.Application.Repositories; // Duplicate removed
using QRCoder;
using EventSphere.Domain.Entities;
using backend.Interfaces;

using EventSphere.Application.Dtos.Payments;
using EventSphere.Domain.Enums;
using EventSphere.Application.Repositories;
namespace EventSphere.Application.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IRegistrationRepository _registrationRepository;
        private readonly IEventService _eventService;
        private readonly IPaymentService _paymentService;
        private readonly backend.Services.IUserService _userService;
        // TODO: Add ITicketService if you have one

        public RegistrationService(
            IRegistrationRepository registrationRepository,
            IEventService eventService,
            IPaymentService paymentService,
            backend.Services.IUserService userService)
        {
            _registrationRepository = registrationRepository;
            _eventService = eventService;
            _paymentService = paymentService;
            _userService = userService;
        }

        /// <summary>
        /// Call this after payment is confirmed to update registration with PaymentId and set PayStatus to Paid.
        /// </summary>
        public async Task<bool> MarkRegistrationPaidAsync(int registrationId, int paymentId)
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId);
            if (registration == null)
                return false;
            registration.PayStatus = PaymentStatus.Paid;
            registration.PaymentId = paymentId;
            // Regenerate QR code with EventTitle, remove PaymentId
            string qrContent = $"RegistrationId: {registration.RegistrationId}, UserId: {registration.UserId}, EventId: {registration.EventId}, EventTitle: {registration.EventTitle}, TicketCount: {registration.TicketCount}";
            string qrCodeBase64 = string.Empty;
            using (var qrGenerator = new QRCoder.QRCodeGenerator())
            {
                var qrData = qrGenerator.CreateQrCode(qrContent, QRCoder.QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new QRCoder.PngByteQRCode(qrData))
                {
                    qrCodeBase64 = Convert.ToBase64String(qrCode.GetGraphic(20));
                }
            }
            registration.QrCode = qrCodeBase64;
            await _registrationRepository.UpdateAsync(registrationId, registration);
            return true;
        }

        public async Task<RegistrationDto> CreateAsync(RegistrationRequestDto registrationDto){
            var eventDetails = await _eventService.GetEventByIdAsync((int)registrationDto.EventId);
            if (eventDetails == null)
                throw new Exception("Event not found");


            // Use email from registration form if provided, else fallback to user profile
            string? userEmail = registrationDto.Email;
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                var userDetails = await _userService.GetUserDetailsByIdAsync(registrationDto.UserId);
                if (userDetails != null)
                {
                    userEmail = userDetails.Email;
                }
            }

            // Prevent registration if max attendees limit is reached
            if (eventDetails.MaxAttendees.HasValue)
            {
                var allRegs = await _registrationRepository.GetAllAsync();
                int currentAttendees = allRegs.Count(r => r.EventId == registrationDto.EventId);
                if (currentAttendees >= eventDetails.MaxAttendees.Value)
                {
                    throw new InvalidOperationException($"Max attendees limit ({eventDetails.MaxAttendees.Value}) reached for event {registrationDto.EventId}.");
                }
            }

            // Allow multiple registrations for the same user and event (no duplicate check)

            int userIdInt = registrationDto.UserId;

            string? paymentIntentId = null;
            int? paymentId = null;
            Registration registration;
            if (eventDetails.Price > 0 && eventDetails.IsPaidEvent)
            {
                // Paid event: create payment intent
                var paymentRequest = new PaymentRequestDto
                {
                    EventId = registrationDto.EventId,
                    TicketCount = registrationDto.TicketCount,
                    UserId = userIdInt
                };
                var paymentResponse = await _paymentService.CreatePaymentIntentAsync(paymentRequest);
                paymentIntentId = paymentResponse.ClientSecret;

                // Create a Payment record with Pending status
                var payment = new Payment
                {
                    UserId = userIdInt,
                    EventId = (int)registrationDto.EventId,
                    Amount = eventDetails.Price.Value * registrationDto.TicketCount,
                    PaymentMethod = "Stripe",
                    TransactionId = paymentIntentId ?? string.Empty,
                    Status = PaymentResultStatus.Pending,
                    PaymentTime = DateTime.UtcNow,
                    EventTitle = eventDetails.Title,
                    UserEmail = userEmail
                };
                if (_paymentService is EventSphere.Application.Services.PaymentService paymentServiceImpl &&
                    paymentServiceImpl.GetType().GetField("_paymentRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(paymentServiceImpl) is EventSphere.Application.Repositories.IPaymentRepository paymentRepo)
                {
                    var addAsyncMethod = paymentRepo.GetType().GetMethod("AddAsync");
                    if (addAsyncMethod != null)
                    {
                        var addTaskObj = addAsyncMethod.Invoke(paymentRepo, new object[] { payment });
                        if (addTaskObj is Task addTask)
                        {
                            await addTask.ConfigureAwait(false);
                            paymentId = payment.PaymentId;
                        }
                    }
                }
                registration = new Registration
                {
                    EventId = (int)registrationDto.EventId,
                    UserId = userIdInt,
                    RegisteredAt = DateTime.UtcNow,
                    Status = 0, // Pending
                    PayStatus = EventSphere.Domain.Enums.PaymentStatus.Pending,
                    PaymentId = paymentId,
                    QrCode = null, // will be set after save
                    EventTitle = eventDetails.Title,
                    UserEmail = userEmail,
                    TicketCount = registrationDto.TicketCount
                };
            }
            else
            {
                // Free event: just create registration
                registration = new Registration
                {
                    EventId = (int)registrationDto.EventId,
                    UserId = userIdInt,
                    RegisteredAt = DateTime.UtcNow,
                    Status = EventSphere.Domain.Enums.RegistrationStatus.Registered,
                    PayStatus = EventSphere.Domain.Enums.PaymentStatus.Free,
                    PaymentId = null,
                    QrCode = null, // will be set after save
                    EventTitle = eventDetails.Title,
                    UserEmail = userEmail,
                    TicketCount = registrationDto.TicketCount
                };
            }

            // Save registration to get RegistrationId
            var created = await _registrationRepository.AddAsync(registration);

            // Now generate QR code with actual RegistrationId and EventTitle (no PaymentId)
            string qrContent = $"RegistrationId: {created.RegistrationId}, UserId: {created.UserId}, EventId: {created.EventId}, EventTitle: {created.EventTitle}, TicketCount: {created.TicketCount}";
            string qrCodeBase64 = string.Empty;
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new PngByteQRCode(qrData))
                {
                    qrCodeBase64 = Convert.ToBase64String(qrCode.GetGraphic(20));
                }
            }
            // Update registration with QR code
            created.QrCode = qrCodeBase64;
            await _registrationRepository.UpdateAsync(created.RegistrationId, created);

            return new RegistrationDto
            {
                Id = created.RegistrationId,
                EventId = created.EventId,
                UserId = created.UserId,
                TicketCount = registrationDto.TicketCount,
                PaymentIntentId = paymentIntentId,
                PaymentId = created.PaymentId,
                RegisteredAt = created.RegisteredAt,
                CheckinTime = created.CheckinTime,
                UserEmail = created.UserEmail,
                QrCode = created.QrCode
            };
        }

        public async Task<IEnumerable<RegistrationDto>> GetAllAsync()
        {
            var regs = await _registrationRepository.GetAllAsync();
            var dtos = new List<RegistrationDto>();
            foreach (var r in regs)
            {
                dtos.Add(new RegistrationDto
                {
                    Id = r.RegistrationId,
                    EventId = r.EventId,
                    UserId = r.UserId,
                    TicketCount = r.TicketCount,
                    PaymentIntentId = null, // Not available in entity, only on creation
                    PaymentId = r.PaymentId,
                    RegisteredAt = r.RegisteredAt,
                    CheckinTime = r.CheckinTime,
                    UserEmail = r.UserEmail,
                    QrCode = r.QrCode,
                    EventTitle = r.EventTitle
                });
            }
            return dtos;
        }

        public async Task<RegistrationDto?> GetByIdAsync(int id)
        {
            var r = await _registrationRepository.GetByIdAsync(id);
            if (r == null) return null;
            return new RegistrationDto
            {
                Id = r.RegistrationId,
                EventId = r.EventId,
                UserId = r.UserId,
                TicketCount = r.TicketCount,
                PaymentIntentId = null, // Not available in entity, only on creation
                PaymentId = r.PaymentId,
                RegisteredAt = r.RegisteredAt,
                CheckinTime = r.CheckinTime,
                UserEmail = r.UserEmail,
                QrCode = r.QrCode,
                EventTitle = r.EventTitle
            };
        }

        public Task<bool> UpdateAsync(int id, RegistrationRequestDto registrationDto)
        {
            // Not implemented
            return Task.FromResult(false);
        }

        public Task<bool> DeleteAsync(int id)
        {
            // Not implemented
            return Task.FromResult(false);
        }
    }
}
          

