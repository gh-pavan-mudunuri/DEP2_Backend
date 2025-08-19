using System.ComponentModel.DataAnnotations;

namespace EventSphere.Application.Dtos.Events
{
    public class EditEventDto
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string CoverImage { get; set; } = string.Empty;

        public string? VibeVideoUrl { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime RegistrationDeadline { get; set; }

        [Required]
        public DateTime EventStart { get; set; }

        [Required]
        public DateTime EventEnd { get; set; }

        public bool IsPaidEvent { get; set; } = false;
        public decimal? Price { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("maxAttendees")]
        public int? MaxAttendees { get; set; }
    }
}
