using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventSphere.Domain.Entities
{
    public class EventFeedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [Required]
        [ForeignKey("Event")]
        public int EventId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Registration")]
        public int? RegistrationId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comments { get; set; }

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // For email/notification context
        public string? UserEmail { get; set; }
        public string? EventTitle { get; set; }

        // Navigation properties
        public virtual Event? Event { get; set; }
        public virtual User? User { get; set; }
        public virtual Registration? Registration { get; set; }
    }
}
