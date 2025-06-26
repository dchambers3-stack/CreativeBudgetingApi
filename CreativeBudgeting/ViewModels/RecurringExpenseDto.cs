namespace CreativeBudgeting.ViewModels
{
    public class RecurringExpenseDto
    {
        public Guid Id { get; set; }
        public string? RecurringExpenseName { get; set; }
        public decimal RecurringAmount { get; set; }
        public int FrequencyId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public int SubcategoryId { get; set; }
        public int PaycheckId { get; set; }

    }
}
