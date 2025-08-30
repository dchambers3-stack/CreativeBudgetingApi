namespace CreativeBudgeting.ViewModels
{
    public class HelpdeskTicketDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Subject { get; set; }
        public int? TicketSeverityId { get; set; }
        public string? Message { get; set; }
        public string? TicketSeverityName { get; set; }
        public bool IsResolved { get; set; }
    }
}
