using System.Security.Cryptography.X509Certificates;

namespace CreativeBudgeting.Models.Seeds
{
    public class RecurringFrequency
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
