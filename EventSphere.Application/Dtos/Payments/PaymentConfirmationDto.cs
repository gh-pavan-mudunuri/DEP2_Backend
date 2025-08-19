namespace EventSphere.Application.Dtos.Payments
{
    public class PaymentConfirmationDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int UserId { get; set; }
        public long EventId { get; set; }
        public string? UserEmail { get; set; }
        public string? EventTitle { get; set; }
    }
}
