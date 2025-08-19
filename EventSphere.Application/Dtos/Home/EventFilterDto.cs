namespace EventSphere.Application.Dtos
{
    public class EventFilterDto
    {
        public string? Location { get; set; }
        public string? Price { get; set; }
        public DateTime? Date { get; set; }
        public string? Category { get; set; }
        public string? RecurrenceType { get; set; }
        public string? EventType { get; set; }

    }
}