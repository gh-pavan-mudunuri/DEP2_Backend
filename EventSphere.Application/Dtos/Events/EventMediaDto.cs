namespace EventSphere.Application.Dtos
{
    public class EventMediaDto
    {
        public string MediaType { get; set; } = string.Empty;
        public string MediaUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
