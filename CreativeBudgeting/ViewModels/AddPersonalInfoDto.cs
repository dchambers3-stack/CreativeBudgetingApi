using System.ComponentModel.DataAnnotations.Schema;
using CreativeBudgeting.Models;

namespace CreativeBudgeting.ViewModels
{
    public class AddPersonalInfoDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        
    }
}
