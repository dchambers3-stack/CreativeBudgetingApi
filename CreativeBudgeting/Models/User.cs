namespace CreativeBudgeting.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Hash { get; set; }

        public byte[]? ProfilePicture { get; set; }
        public string? ProfilePictureContentType { get; set; }

        public virtual ICollection<PersonalInfo>? PersonalInfos { get; set; }
        public virtual ICollection<Expense>? Expenses { get; set; }
        public virtual ICollection<Paycheck> Paychecks { get; set; } = new List<Paycheck>();

    }
}
