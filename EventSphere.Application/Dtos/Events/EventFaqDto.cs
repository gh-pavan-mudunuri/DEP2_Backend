namespace EventSphere.Application.Dtos
{
    public class EventFaqDto
    {
        public int FaqId { get; set; } = 0;
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
    }
}
