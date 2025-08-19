namespace EventSphere.Application.Dtos.Payments
{
    public class CheckoutSessionRequestDto
    {
    public long EventId { get; set; }
    public int TicketCount { get; set; }
    public int UserId { get; set; }
    public string? SuccessUrl { get; set; }
    public string? CancelUrl { get; set; }
    public string? Currency { get; set; }
    public string? Email { get; set; } // User's email for Stripe receipt
    public string? UserEmail { get; set; } // For storing in Payment
    public string? EventTitle { get; set; } // For storing in Payment
    }

    public class CheckoutSessionResponseDto
    {
        public string Url { get; set; } = string.Empty;
    }
}
