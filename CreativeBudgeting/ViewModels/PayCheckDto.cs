using System.ComponentModel.DataAnnotations;
using CreativeBudgeting.Models;

namespace CreativeBudgeting.ViewModels
{
    public class PayCheckDto
    {
      
       public int Id { get; set; }
        public int UserId { get; set; }

        
        public DateTime PayDate { get; set; }

       
        public double Amount { get; set; }
       
    }
}
