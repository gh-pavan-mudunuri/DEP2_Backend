using System.Threading.Tasks;
using EventSphere.Application.Dtos.Payments;

namespace EventSphere.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> CreatePaymentIntentAsync(PaymentRequestDto request);
        Task<CheckoutSessionResponseDto> CreateCheckoutSessionAsync(CheckoutSessionRequestDto request);
    Task UpdatePaymentAndRegistrationAsync(string paymentIntentId, System.Collections.Generic.IDictionary<string, string> metadata, decimal amount);
    Task<bool> ConfirmAndUpdateAsync(string paymentIntentId, string transactionId, string status, decimal amount, int userId, long eventId, string? userEmail = null, string? eventTitle = null);
    }
}
