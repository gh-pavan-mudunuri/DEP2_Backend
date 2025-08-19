using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EventSphere.Domain.Entities
{
    public class EventSpeaker
    {
        [Key]
        public int SpeakerId { get; set; }

        [Required]
        [ForeignKey("Event")]
        public int EventId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }
        public string? Bio { get; set; }
        public string? SocialLinks { get; set; }

        // Navigation and workflow fields
        public string? EventTitle { get; set; }

        // Navigation property
        public virtual Event? Event { get; set; }
    }
}
