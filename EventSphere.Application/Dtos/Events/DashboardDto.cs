using System.ComponentModel.DataAnnotations;

namespace EventSphere.Application.Dtos
{
    public class DashboardDto
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        // Add more dashboard fields as needed
    }
}
