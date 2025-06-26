using CreativeBudgeting.Models;
using Microsoft.EntityFrameworkCore;

namespace CreativeBudgeting.Services
{
    public class RecurringService
    {
        private readonly BudgetDbContext _context;

        public RecurringService(BudgetDbContext context)
        {
            _context = context;
        }

        public async Task GenerateMonthlyRecurringExpensesAsync()
        {
            var today = DateTime.Today;
            var recurringExpenses = await _context.RecurringExpenses.ToListAsync();
            var defaultCategory = await _context.Categories.FirstOrDefaultAsync();
            var defaultSubCategory = await _context.Subcategories.FirstOrDefaultAsync();
            if (defaultCategory == null)
            {
                throw new InvalidOperationException("No categories found in the database.");
            }
            if (defaultSubCategory == null)
            {
                throw new InvalidOperationException("No subcategories found in the database.");
            }


            foreach (var re in recurringExpenses)
            {
                Console.WriteLine($"Checking recurring expense: {re.RecurringExpenseName} for user {re.UserId}");

                var expenses = await _context
                    .Expenses.Where(e => e.RecurringExpenseId == re.Id)
                    .ToListAsync();

                var existsThisMonth = expenses.Any(e =>
                    DateTime.TryParse(e.DueDate, out var dueDate) &&
                    dueDate.Month == today.Month &&
                    dueDate.Year == today.Year
                );
                Console.WriteLine($"Already exists this month? {existsThisMonth}");

                if (!existsThisMonth)
                {
                    var newExpense = new Expense
                    {
                        UserId = re.UserId,
                        Name = re.RecurringExpenseName,
                        Payment = (double)re.RecurringAmount,
                        DueDate = today.ToString("yyyy-MM-dd"),
                        RecurringExpenseId = re.Id,
                        IsPaid = false,
                        // Optionally set CategoryId, SubcategoryId if applicable
                        CategoryId = re.CategoryId ?? defaultCategory.Id,
                        SubcategoryId = re.SubcategoryId ?? defaultSubCategory.Id,
                        PaycheckId = re.PaycheckId
                        
                    };

                    _context.Expenses.Add(newExpense);
                    Console.WriteLine($"Created expense: {newExpense.Name}");

                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
