using System.Text.Json;
using CreativeBudgeting.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;

namespace CreativeBudgeting
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await Task.CompletedTask;
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddHangfireServer();
            builder.Services.AddScoped<RecurringService>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();
            builder
                .Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddHangfire(config =>
            {
                config.UsePostgreSqlStorage(connectionString);
            });

            builder.Services.AddDbContext<BudgetDbContext>(options =>
                options.UseNpgsql(connectionString)
            );

            builder.Services.AddScoped<PasswordService>();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });

            builder.Services.AddCors(opt =>
            {
                opt.AddPolicy(
                    "AllowLocalHost",
                    policy =>
                    {
                        policy
                            .WithOrigins(
                                "http://localhost:4200",
                                "http://localhost:5000",
                                "http://localhost:60176"
                            )
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    }
                );
            });

            var app = builder.Build();
            // Schedule (1st of every month)

            app.UseCors("AllowLocalHost"); // Ensure CORS is set before UseAuthorization
            app.UseSession();
            //app.UseAuthorization();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.MapControllers();
            app.UseHangfireDashboard(); // Add Hangfire Dashboard middleware
            RecurringJob.AddOrUpdate<RecurringService>(
                "generate-monthly-recurring-expenses", // Unique job ID
                service => service.GenerateMonthlyRecurringExpensesAsync(), // Method to run
                Cron.Monthly // testing purposes
            );

            app.Run();
        }
    }
}
