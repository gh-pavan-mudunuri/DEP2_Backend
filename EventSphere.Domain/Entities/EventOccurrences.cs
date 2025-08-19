using System;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventSphere.Domain.Entities
{
    public class EventOccurrence
    {
        [Key]
        public int OccurrenceId { get; set; }

        [Required]
        [ForeignKey("Event")]
        public int EventId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        // Navigation and workflow fields
        public string? EventTitle { get; set; }

        public bool IsCancelled { get; set; } = false;

        // Navigation property
        public virtual Event? Event { get; set; }
    }
}
