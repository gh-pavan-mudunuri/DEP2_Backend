using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using EventSphere.Domain.Enums;
namespace EventSphere.Domain.Entities
{
    // Enum moved to Enums/EventInvitationEnums.cs
    public class EventInvitation
    {
        [Key]
        public int InvitationId { get; set; }

        [Required]
        [ForeignKey("Event")]
        public int EventId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int InvitedUserId { get; set; }

        [Required]
        [EmailAddress]
        public string InvitedEmail { get; set; } = string.Empty;

        [Required]
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }
        public string? ResponseMessage { get; set; }

        // For email/notification context
        public string? EventTitle { get; set; }
        public string? OrganizerEmail { get; set; }

        // Navigation properties
        public virtual Event? Event { get; set; }
        public virtual User? InvitedUser { get; set; }
    }
}
