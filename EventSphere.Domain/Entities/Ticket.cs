using System;
using System.Collections.Generic;
using EventSphere.Domain.Enums;
namespace EventSphere.Domain.Entities
{
    // Enums moved to Enums/TicketEnums.cs
    public class Ticket
    {
        public int TicketId { get; set; }
        public int EventId { get; set; }
        public string? UserEmail { get; set; } // For sending ticket info/updates
        public TicketType Type { get; set; } = TicketType.General;
        public decimal? Price { get; set; }
        public int Quantity { get; set; }
        public int AvailableTickets { get; set; }
        public TicketStatus Status { get; set; } = TicketStatus.Available;
        public DateTime? IssuedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? QRCode { get; set; } // For validation at entry
        public string? Notes { get; set; } // For admin or user notes
    }
}
