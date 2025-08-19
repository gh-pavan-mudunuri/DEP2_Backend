        // Stub implementation to satisfy interface. Update logic as needed.
using System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Application.Dtos.Payments;
using EventSphere.Application.Interfaces;

namespace EventSphere.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly EventSphere.Application.Repositories.IPaymentRepository _paymentRepository;

        public PaymentService(EventSphere.Application.Repositories.IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentResponseDto> CreatePaymentIntentAsync(PaymentRequestDto request)
        {
            return await _paymentRepository.CreatePaymentIntentAsync(request);
        }

        public async Task<CheckoutSessionResponseDto> CreateCheckoutSessionAsync(CheckoutSessionRequestDto request)
        {
            return await _paymentRepository.CreateCheckoutSessionAsync(request);
        }

        public async Task UpdatePaymentAndRegistrationAsync(string paymentIntentId, IDictionary<string, string> metadata, decimal amount)
        {
            Console.WriteLine($"[Service] UpdatePaymentAndRegistrationAsync called for PaymentIntentId: {paymentIntentId}");
            foreach (var kv in metadata)
            {
                Console.WriteLine($"[Service] Metadata: {kv.Key} = {kv.Value}");
            }
            if (!metadata.TryGetValue("userId", out var userIdStr) || !int.TryParse(userIdStr, out var userId) ||
                !metadata.TryGetValue("eventId", out var eventIdStr) || !long.TryParse(eventIdStr, out var eventId))
            {
                Console.WriteLine("[Service] Missing or invalid required metadata for payment update.");
                throw new InvalidOperationException("Missing or invalid required metadata for payment update.");
            }
            var result = await _paymentRepository.ConfirmAndUpdateAsync(
                paymentIntentId, paymentIntentId, "succeeded", amount, userId, eventId);
            Console.WriteLine($"[Service] ConfirmAndUpdateAsync result: {result}");
        }

        // Stub implementation to satisfy interface. Update logic as needed.
        public Task<bool> ConfirmAndUpdateAsync(string paymentIntentId, string transactionId, string status, decimal amount, int userId, long eventId, string? extra1, string? extra2)
        {
            // TODO: Implement actual logic or remove from interface if not needed
            return Task.FromResult(false);
        }
    }
}
