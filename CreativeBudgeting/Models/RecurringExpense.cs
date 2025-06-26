using System.ComponentModel.DataAnnotations.Schema;
using CreativeBudgeting.Models.Seeds;

namespace CreativeBudgeting.Models
{
    public class RecurringExpense
    {
        public Guid Id { get; set; }
        public string RecurringExpenseName { get; set; } = "";
        public decimal RecurringAmount { get; set; }
        public int FrequencyId { get; set; }
        public int UserId { get; set; }

        public int? PaycheckId { get; set; }
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        [ForeignKey(nameof(SubcategoryId))]
        public Subcategory? Subcategory { get; set; }

        [ForeignKey(nameof(PaycheckId))]
        public Paycheck? Paycheck { get; set; }



        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [ForeignKey(nameof(FrequencyId))]
        public RecurringFrequency? Frequency { get; set; } 

    }
}
