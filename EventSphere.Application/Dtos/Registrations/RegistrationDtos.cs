using System;

namespace EventSphere.Application.Dtos.Registrations
{
    public class RegistrationDto
    {
        public int Id { get; set; }
        public long EventId { get; set; }
        public int UserId { get; set; }
        public int TicketCount { get; set; }
        public string? PaymentIntentId { get; set; }
        public int? PaymentId { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? CheckinTime { get; set; }
        public string? UserEmail { get; set; }
        public string? QrCode { get; set; } // base64 PNG string
        public string? EventTitle { get; set; }
    }

    public class RegistrationRequestDto
    {
    public long EventId { get; set; }
    public int UserId { get; set; }
    public int TicketCount { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? Email { get; set; } // Added: email from registration form
    }
}
