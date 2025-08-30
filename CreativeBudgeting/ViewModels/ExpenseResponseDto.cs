namespace CreativeBudgeting.ViewModels
{
    public class ExpenseResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Name { get; set; }
        public double Payment { get; set; }
        public double? TotalBalance { get; set; }
        public string? DueDate { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int SubcategoryId { get; set; }
        public string? SubcategoryName { get; set; }
        public int? PaycheckId { get; set; }
        public bool IsPaid { get; set; }
    }

}
