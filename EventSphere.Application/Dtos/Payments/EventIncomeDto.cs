namespace EventSphere.Application.Dtos.Payments
{
    public class EventIncomeDto
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = "";
        public decimal TotalAmount { get; set; }
    }
}
