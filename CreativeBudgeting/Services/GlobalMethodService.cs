using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CreativeBudgeting.Services
{
    public class GlobalMethodService
    {
        private readonly BudgetDbContext _context;

        public GlobalMethodService(BudgetDbContext context)
        {
            _context = context;
        }

        public async Task MarkAllExpensesUnpaidAsync()
        {
            var expenses = await _context.Expenses.ToListAsync();
            foreach (var expense in expenses)
            {
                expense.IsPaid = false; // Adjust property name if different
                if(expense.CategoryId == 10)
                {
                    _context.Remove(expense);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task MarkExpensesUnpaidByUserId(int userId)
        {
            var expenses = await _context.Expenses.Where(e => e.UserId == userId).ToListAsync();
            foreach (var expense in expenses)
            {
                expense.IsPaid = false;
                if (expense.CategoryId == 10)
                {
                    _context.Remove(expense);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
