using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventSphere.Domain.Entities
{
    public class EventFaq
    {
        [Key]
        public int FaqId { get; set; }

        [Required]
        [ForeignKey("Event")]
        public int EventId { get; set; }

        [Required]
        public string Question { get; set; } = string.Empty;

        [Required]
        public string Answer { get; set; } = string.Empty;

        // Navigation and workflow fields
        public string? EventTitle { get; set; }

        // Navigation property
        public virtual Event? Event { get; set; }
    }
}
