namespace EventSphere.Application.Dtos.Payments
{

    public class PaymentRequestDto
    {
        public long EventId { get; set; }
        public int TicketCount { get; set; }
        public int UserId { get; set; }
        public string? Email { get; set; } // User's email for Stripe receipt
        public string? UserEmail { get; set; } // For storing in Payment
        public string? EventTitle { get; set; } // For storing in Payment
    }

    public class PaymentResponseDto
    {
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}
