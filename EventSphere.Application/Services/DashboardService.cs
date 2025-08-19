using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSphere.Application.Dtos;
using EventSphere.Application.Interfaces;
using EventSphere.Application.Repositories;
using EventSphere.Domain.Entities;
using EventSphere.Infrastructure.Repositories;

namespace EventSphere.Application.Services
{
    public class DashboardService : IDashboardService
    {
    private readonly IEventRepository _eventRepository;
    private readonly IPaymentRepository _paymentRepository;


        public DashboardService(IEventRepository eventRepository, IPaymentRepository paymentRepository)
        {
            _eventRepository = eventRepository;
            _paymentRepository = paymentRepository;
        }
        public async Task<IEnumerable<EventSphere.Application.Dtos.Events.OrganizedEventPaymentsDto>> GetOrganizedEventsPaymentsAsync(int organizerId)
        {
            var events = (await _eventRepository.GetAllEventsAsync())
                .Where(e => e.OrganizerId == organizerId);

            var result = new List<EventSphere.Application.Dtos.Events.OrganizedEventPaymentsDto>();
            foreach (var ev in events)
            {
                var payments = await _paymentRepository.GetPaymentsForEventAsync(ev.EventId);
                var totalAmount = payments.Sum(p => p.Amount);
                var organizerIncome = totalAmount * 0.9m;
                var commission = totalAmount * 0.1m;

                result.Add(new EventSphere.Application.Dtos.Events.OrganizedEventPaymentsDto
                {
                    EventId = ev.EventId,
                    EventTitle = ev.Title,
                    TotalAmount = totalAmount,
                    OrganizerIncome = organizerIncome,
                    Commission = commission,
                    Payments = payments.Select(p => new EventSphere.Application.Dtos.Events.PaymentDto
                    {
                        PaymentId = p.PaymentId,
                        UserEmail = p.UserEmail ?? "",
                        Amount = p.Amount,
                        Status = p.Status.ToString(),
                        PaymentTime = p.PaymentTime
                    }).ToList()
                });
            }
            return result;
        }

        public async Task<IEnumerable<EventCardDto>> GetCurrentOrganizedEventsAsync(int organizerId)
        {
             return await _eventRepository.GetCurrentOrganizedEventsAsync(organizerId);
        }

        public async Task<IEnumerable<EventCardDto>> GetPastOrganizedEventsAsync(int organizerId)
        {
            var events = (await _eventRepository.GetAllEventsAsync())
            .Where(e => e.OrganizerId == organizerId && e.IsCompleted == 1);

            var eventCardDtos = new List<EventCardDto>();

            foreach (var e in events)
            {
                var registrationCount = await _eventRepository.GetRegistrationCountForEventAsync(e.EventId);

                eventCardDtos.Add(new EventCardDto
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    CoverImage = e.CoverImage,
                    Category = e.Category,
                    EventType = e.EventType,
                    EditEventCount = e.EditEventCount,
                    Location = e.Location,
                    RegistrationDeadline = e.RegistrationDeadline,
                    EventStart = e.EventStart,
                    EventEnd = e.EventEnd,
                    Price = e.Price,
                    RegistrationCount = registrationCount
                });
            }

            return eventCardDtos;
        }

        public async Task<IEnumerable<EventCardDto>> GetCurrentAttendingEventsAsync(int userId)
{
    // 1. Get all event IDs user has registered for
    var registeredEventIds = await _eventRepository.GetRegisteredEventIdsByUserIdAsync(userId);

    // 2. Get full events and filter by IsCompleted == 0
    var events = (await _eventRepository.GetAllEventsAsync())
        .Where(e => registeredEventIds.Contains(e.EventId) && e.IsCompleted == 0);

    // 3. Create DTO list
    var eventCardDtos = new List<EventCardDto>();
    foreach (var e in events)
    {
        var registrationCount = await _eventRepository.GetRegistrationCountForEventAsync(e.EventId);

        eventCardDtos.Add(new EventCardDto
        {
            EventId = e.EventId,
            Title = e.Title,
            CoverImage = e.CoverImage,
            Category = e.Category,
            EventType = e.EventType,
            Location = e.Location,
            EditEventCount = e.EditEventCount,
            RegistrationDeadline = e.RegistrationDeadline,
            EventStart = e.EventStart,
            EventEnd = e.EventEnd,
            Price = e.Price,
            RegistrationCount = registrationCount
        });
    }

    return eventCardDtos;
}

public async Task<IEnumerable<EventCardDto>> GetPastAttendedEventsAsync(int userId)
{
    var registeredEventIds = await _eventRepository.GetRegisteredEventIdsByUserIdAsync(userId);

    var events = (await _eventRepository.GetAllEventsAsync())
        .Where(e => registeredEventIds.Contains(e.EventId) && e.IsCompleted == 1);

    var eventCardDtos = new List<EventCardDto>();
    foreach (var e in events)
    {
        var registrationCount = await _eventRepository.GetRegistrationCountForEventAsync(e.EventId);

        eventCardDtos.Add(new EventCardDto
        {
            EventId = e.EventId,
            Title = e.Title,
            CoverImage = e.CoverImage,
            Category = e.Category,
            EventType = e.EventType,
            Location = e.Location,
            RegistrationDeadline = e.RegistrationDeadline,
            EventStart = e.EventStart,
            EditEventCount = e.EditEventCount,
            EventEnd = e.EventEnd,
            Price = e.Price,
            RegistrationCount = registrationCount
        });
    }

    return eventCardDtos;
}



        
    }
}
