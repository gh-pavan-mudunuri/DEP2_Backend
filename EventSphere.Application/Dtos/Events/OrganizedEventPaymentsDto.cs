using System;
using System.Collections.Generic;

namespace EventSphere.Application.Dtos.Events
{
    public class OrganizedEventPaymentsDto
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal OrganizerIncome { get; set; }
        public decimal Commission { get; set; }
        public List<PaymentDto> Payments { get; set; } = new();
    }

    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime PaymentTime { get; set; }
    }
}
