namespace CreativeBudgeting.ViewModels
{
    public class BudgetPeriodDto
    {

        public int? UserId { get; set; }
        public string? Name { get; set; }
        public string? NextMonthName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
