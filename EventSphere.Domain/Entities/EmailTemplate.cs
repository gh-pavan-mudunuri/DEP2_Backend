using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using EventSphere.Domain.Enums;
namespace EventSphere.Domain.Entities
{
    // Enum moved to Enums/EmailTemplateEnums.cs
    public class EmailTemplate
    {
        [Key]
        public int TemplateId { get; set; }

        [Required]
        public EmailTemplateType Type { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        [ForeignKey("Event")]
        public int? EventId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual Event? Event { get; set; }
    }
}
