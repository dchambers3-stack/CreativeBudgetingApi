using CreativeBudgeting.Models;
using CreativeBudgeting.Models.Seeds;
using Microsoft.EntityFrameworkCore;

namespace CreativeBudgeting
{
    public class BudgetDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<PersonalInfo> PersonalInfo { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }
        public DbSet<Paycheck> Paychecks { get; set; }
        public DbSet<RecurringFrequency> RecurringFrequencies { get; set; }
        public DbSet<RecurringExpense> RecurringExpenses { get; set; }


        public BudgetDbContext(DbContextOptions<BudgetDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<RecurringFrequency>().Property(r => r.Id).ValueGeneratedNever();

            modelBuilder.Entity<RecurringFrequency>().HasData(
            new RecurringFrequency
            {
                Id = 1,
                Name = "Daily",
                Value = "daily",
                SortOrder = 1
            },
            new RecurringFrequency
            {
                Id = 2,
                Name = "Weekly",
                Value = "weekly",
                SortOrder = 2
            },
            new RecurringFrequency
            {
                Id = 3,
                Name = "Bi-Weekly",
                Value = "biweekly",
                SortOrder = 3
            },
            new RecurringFrequency
            {
                Id = 4,
                Name = "Monthly",
                Value = "monthly",
                SortOrder = 4
            },
            new RecurringFrequency
            {
                Id = 5,
                Name = "Yearly",
                Value = "yearly",
                SortOrder = 5
            }
            );

            modelBuilder.Entity<PersonalInfo>(entity =>
            {
                entity.ToTable("personal_info");
                entity.Property(e => e.UserId).HasColumnName("user_id");
               
                entity.Property(e => e.FirstName).HasColumnName("first_name");
                entity.Property(e => e.LastName).HasColumnName("last_name");
            });

            modelBuilder.Entity<RecurringExpense>(entity =>
            {
                entity.ToTable("recurring_expenses");
                entity.Property(e => e.FrequencyId).HasColumnName("frequency_id");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.RecurringExpenseName).HasColumnName("recurring_expense_name");
                entity.Property(e => e.RecurringAmount).HasColumnName("recurring_amount");

            });

            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Food" },
                new Category { Id = 2, Name = "Utilities" },
                new Category { Id = 3, Name = "Entertainment" },
                new Category { Id = 4, Name = "Housing" },
                new Category { Id = 5, Name = "Transportation" },
                new Category { Id = 6, Name = "Health" },
                new Category { Id = 7, Name = "Personal Care" },
                new Category { Id = 8, Name = "Education" },
                new Category { Id = 9, Name = "Debt" },
                new Category { Id = 10, Name = "Savings and Investments" },
                new Category { Id = 11, Name = "Gifts and Donations" },
                new Category { Id = 12, Name = "Miscellaneous" }
            );

            // Subcategories
            modelBuilder.Entity<Subcategory>().HasData(
                // Food
                new Subcategory { Id = 1, Name = "Groceries", CategoryId = 1 },
                new Subcategory { Id = 2, Name = "Dining Out", CategoryId = 1 },
                new Subcategory { Id = 3, Name = "Snacks", CategoryId = 1 },
                new Subcategory { Id = 4, Name = "Coffee/Tea", CategoryId = 1 },
                new Subcategory { Id = 5, Name = "Fast Food", CategoryId = 1 },

                // Utilities
                new Subcategory { Id = 6, Name = "Electricity", CategoryId = 2 },
                new Subcategory { Id = 7, Name = "Water", CategoryId = 2 },
                new Subcategory { Id = 8, Name = "Internet", CategoryId = 2 },
                new Subcategory { Id = 9, Name = "Gas", CategoryId = 2 },
                new Subcategory { Id = 10, Name = "Trash Collection", CategoryId = 2 },
                new Subcategory { Id = 11, Name = "Heating/Cooling", CategoryId = 2 },

                // Entertainment
                new Subcategory { Id = 12, Name = "Movies", CategoryId = 3 },
                new Subcategory { Id = 13, Name = "Music", CategoryId = 3 },
                new Subcategory { Id = 14, Name = "Sports", CategoryId = 3 },
                new Subcategory { Id = 15, Name = "Gaming", CategoryId = 3 },
                new Subcategory { Id = 16, Name = "Books", CategoryId = 3 },
                new Subcategory { Id = 17, Name = "TV Shows", CategoryId = 3 },
                new Subcategory { Id = 18, Name = "Concerts", CategoryId = 3 },

                // Housing
                new Subcategory { Id = 19, Name = "Rent", CategoryId = 4 },
                new Subcategory { Id = 20, Name = "Mortgage", CategoryId = 4 },
                new Subcategory { Id = 21, Name = "Home Repairs", CategoryId = 4 },
                new Subcategory { Id = 22, Name = "Property Taxes", CategoryId = 4 },
                new Subcategory { Id = 23, Name = "Home Insurance", CategoryId = 4 },
                new Subcategory { Id = 24, Name = "Lawn Care", CategoryId = 4 },

                // Transportation
                new Subcategory { Id = 25, Name = "Gas", CategoryId = 5 },
                new Subcategory { Id = 26, Name = "Public Transportation", CategoryId = 5 },
                new Subcategory { Id = 27, Name = "Car Insurance", CategoryId = 5 },
                new Subcategory { Id = 28, Name = "Parking", CategoryId = 5 },
                new Subcategory { Id = 29, Name = "Tolls", CategoryId = 5 },
                new Subcategory { Id = 30, Name = "Vehicle Maintenance", CategoryId = 5 },
                new Subcategory { Id = 31, Name = "Car Payment", CategoryId = 5 },

                // Health
                new Subcategory { Id = 32, Name = "Health Insurance", CategoryId = 6 },
                new Subcategory { Id = 33, Name = "Doctor Visits", CategoryId = 6 },
                new Subcategory { Id = 34, Name = "Dentist Visits", CategoryId = 6 },
                new Subcategory { Id = 35, Name = "Prescriptions", CategoryId = 6 },
                new Subcategory { Id = 36, Name = "Gym Membership", CategoryId = 6 },
                new Subcategory { Id = 37, Name = "Mental Health", CategoryId = 6 },
                new Subcategory { Id = 38, Name = "Eyewear", CategoryId = 6 },

                // Personal Care
                new Subcategory { Id = 39, Name = "Haircuts", CategoryId = 7 },
                new Subcategory { Id = 40, Name = "Skincare", CategoryId = 7 },
                new Subcategory { Id = 41, Name = "Toiletries", CategoryId = 7 },
                new Subcategory { Id = 42, Name = "Clothing", CategoryId = 7 },
                new Subcategory { Id = 43, Name = "Beauty Products", CategoryId = 7 },
                new Subcategory { Id = 44, Name = "Spa Treatments", CategoryId = 7 },

                // Education
                new Subcategory { Id = 45, Name = "Tuition", CategoryId = 8 },
                new Subcategory { Id = 46, Name = "Books and Supplies", CategoryId = 8 },
                new Subcategory { Id = 47, Name = "Online Courses", CategoryId = 8 },
                new Subcategory { Id = 48, Name = "Certification Fees", CategoryId = 8 },
                new Subcategory { Id = 49, Name = "School Fees", CategoryId = 8 },

                // Debt
                new Subcategory { Id = 50, Name = "Credit Card Payments", CategoryId = 9 },
                new Subcategory { Id = 51, Name = "Loan Repayments", CategoryId = 9 },
                new Subcategory { Id = 52, Name = "Student Loans", CategoryId = 9 },
                new Subcategory { Id = 53, Name = "Personal Loans", CategoryId = 9 },

                // Savings and Investments
                new Subcategory { Id = 54, Name = "Emergency Fund", CategoryId = 10 },
                new Subcategory { Id = 55, Name = "Retirement Savings", CategoryId = 10 },
                new Subcategory { Id = 56, Name = "Investments", CategoryId = 10 },
                new Subcategory { Id = 57, Name = "College Fund", CategoryId = 10 },

                // Gifts and Donations
                new Subcategory { Id = 58, Name = "Birthday Gifts", CategoryId = 11 },
                new Subcategory { Id = 59, Name = "Holiday Gifts", CategoryId = 11 },
                new Subcategory { Id = 60, Name = "Charitable Donations", CategoryId = 11 },
                new Subcategory { Id = 61, Name = "Special Occasions", CategoryId = 11 },

                // Miscellaneous
                new Subcategory { Id = 62, Name = "Pet Care", CategoryId = 12 },
                new Subcategory { Id = 63, Name = "Subscriptions", CategoryId = 12 },
                new Subcategory { Id = 64, Name = "Household Supplies", CategoryId = 12 },
                new Subcategory { Id = 65, Name = "Travel Expenses", CategoryId = 12 }
            );
        }
    }
}
