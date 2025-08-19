namespace EventSphere.Application.Dtos
{
    public class AddBookmarkDto
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public string? UserEmail { get; set; }
        public string? EventTitle { get; set; }
    }

}