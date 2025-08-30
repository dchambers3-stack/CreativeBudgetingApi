using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CreativeBudgeting.Models
{
    public class Paycheck
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
       

        [Required]
        public DateTime PayDate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double Amount { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
        public List<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
