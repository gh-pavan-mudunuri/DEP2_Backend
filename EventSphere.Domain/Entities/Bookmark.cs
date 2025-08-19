
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventSphere.Domain.Entities;

namespace EventSphere.Domain.Entities
{
    public class Bookmark
    {
        [Key]
        public int BookmarkId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation and workflow fields
        public string? UserEmail { get; set; } // For bookmark notification
        public string? EventTitle { get; set; } // For email context

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual Event? Event { get; set; }
    }
}
