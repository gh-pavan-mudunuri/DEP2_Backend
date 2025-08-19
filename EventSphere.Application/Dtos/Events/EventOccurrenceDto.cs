using System;

namespace EventSphere.Application.Dtos
{
    public class EventOccurrenceDto
    {
        public int OccurrenceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? EventTitle { get; set; }
        public bool IsCancelled { get; set; }
    }
}
