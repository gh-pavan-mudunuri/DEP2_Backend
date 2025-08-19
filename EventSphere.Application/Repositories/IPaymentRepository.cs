using System.Threading.Tasks;
using EventSphere.Application.Dtos.Payments;
using System.Collections.Generic;

namespace EventSphere.Application.Repositories
{
    public interface IPaymentRepository
    {
        Task<PaymentResponseDto> CreatePaymentIntentAsync(PaymentRequestDto request);
        Task<CheckoutSessionResponseDto> CreateCheckoutSessionAsync(CheckoutSessionRequestDto request);
        Task<bool> ConfirmAndUpdateAsync(string paymentIntentId, string transactionId, string status, decimal amount, int userId, long eventId);
        Task<(string AccountId, string OnboardingUrl)> CreateStripeExpressAccountAsync(string email, string returnUrl, string refreshUrl);
        Task<bool> IsStripeAccountOnboardingCompleteAsync(string stripeAccountId);
        Task<List<EventSphere.Domain.Entities.Payment>> GetPaymentsForEventAsync(int eventId);
        Task<List<EventSphere.Domain.Entities.Payment>> GetAllEventPaymentsAsync();
        Task<List<EventSphere.Application.Dtos.Payments.EventIncomeDto>> GetEventIncomeSummaryAsync();
    }
}