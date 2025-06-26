using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CreativeBudgeting.Models
{
    public class Expense
    {
        public int Id { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        
        public string? Name { get; set; }
        public double Payment { get; set; }
        [Column("due_date")]
        public string? DueDate { get; set; }

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User? User { get; set; }
        public int SubcategoryId { get; set; }
        [ForeignKey("SubcategoryId")]

        public virtual Subcategory? Subcategory { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
        public int? PaycheckId { get; set; }
        [ForeignKey("PaycheckId")]
        public Paycheck? Paycheck { get; set; }
        public Guid? RecurringExpenseId { get; set; }

        [ForeignKey(nameof(RecurringExpenseId))]
        public virtual RecurringExpense? RecurringExpense { get; set; }
        public bool IsPaid { get; set; } = false;


    }
}
