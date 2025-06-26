namespace CreativeBudgeting.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<Subcategory> Subcategories { get; set; } = new();
    }
}
