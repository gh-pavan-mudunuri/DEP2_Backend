using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using EventSphere.Domain.Enums;
namespace EventSphere.Domain.Entities
{
    // Enum moved to Enums/EventMediaEnums.cs
    public class EventMedia
    {
        [Key]
        public int MediaId { get; set; }

        [Required]
        [ForeignKey("Event")]
        public int EventId { get; set; }

        [Required]
        public string MediaUrl { get; set; } = string.Empty;

        [Required]
        public MediaType MediaType { get; set; }

        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation and workflow fields
        public string? EventTitle { get; set; }

        // Navigation property
        public virtual Event? Event { get; set; }
    }
}
