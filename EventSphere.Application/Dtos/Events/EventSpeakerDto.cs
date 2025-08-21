namespace EventSphere.Application.Dtos
{
    public class EventSpeakerDto

    {
        public int SpeakerId { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
