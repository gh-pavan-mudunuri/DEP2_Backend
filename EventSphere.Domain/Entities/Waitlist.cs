using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventSphere.Domain.Entities
{
    public class Waitlist
    {
        [Key]
        public int WaitlistId { get; set; }

        [Required]
        [ForeignKey("Event")]
        public int EventId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public bool Notified { get; set; } = false;

        // For email/notification context
        public string? UserEmail { get; set; }
        public string? EventTitle { get; set; }

        // Navigation properties
        public virtual Event? Event { get; set; }
        public virtual User? User { get; set; }
    }
}
