using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventSphere.Domain.Entities
{
    public class WebsiteReview
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

    public string? Title { get; set; }
    public string? Comments { get; set; }

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
