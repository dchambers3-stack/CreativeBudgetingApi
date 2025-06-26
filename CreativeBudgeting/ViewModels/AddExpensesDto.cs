namespace CreativeBudgeting.ViewModels
{
    public class AddExpensesDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double Payment { get; set; }
        public string? DueDate { get; set; }
        public int UserId { get; set; }
        public int SubcategoryId { get; set; }
     
        public int CategoryId { get; set; }
        public string? PaycheckDate { get; set; }
        public bool IsPaid { get; set; }
        
       
    }
}
