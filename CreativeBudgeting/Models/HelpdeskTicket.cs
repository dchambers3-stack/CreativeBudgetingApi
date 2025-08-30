using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace CreativeBudgeting.Models
{
    public class HelpdeskTicket
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? TicketSeverityId { get; set; }

        [ForeignKey(nameof(TicketSeverityId))]
        public TicketSeverity? TicketSeverity { get; set; }
        public bool IsResolved { get; set; }

    }
}
