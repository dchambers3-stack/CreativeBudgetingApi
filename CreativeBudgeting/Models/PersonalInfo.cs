using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CreativeBudgeting.Models
{
    public class PersonalInfo
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        [JsonIgnore]
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
