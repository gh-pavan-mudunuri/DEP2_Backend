using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using EventSphere.Domain.Enums;
namespace EventSphere.Domain.Entities
{
    // Enums moved to Enums/NotificationEnums.cs
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [ForeignKey("Event")]
        public int EventId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public NotificationStatus Status { get; set; } = NotificationStatus.Unread;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation and workflow fields
        public string? UserEmail { get; set; }
        public string? EventTitle { get; set; }
        public string? ActionUrl { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual Event? Event { get; set; }
    }
}
