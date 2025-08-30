using System.ComponentModel.DataAnnotations.Schema;

namespace CreativeBudgeting.Models
{
    public class Savings
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
