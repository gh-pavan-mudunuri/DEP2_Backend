using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventSphere.Domain.Entities
{
    public class EventLog
    {
        [Key]
        public int LogId { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }

        [ForeignKey("Event")]
        public int? EventId { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty; // e.g., "CreatedEvent", "PurchasedTicket"

        public string? Details { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? UserEmail { get; set; }
        public string? EventTitle { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual Event? Event { get; set; }
    }
}
