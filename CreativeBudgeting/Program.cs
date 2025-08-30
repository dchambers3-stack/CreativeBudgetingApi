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
            var builder = WebApplication.CreateBuilder(args);

            // Configure Kestrel for Render
            builder.WebHost.ConfigureKestrel(options =>
            {
                var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
                options.ListenAnyIP(int.Parse(port));
            });

            // Add services to the container
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            // Configure CORS for Render deployment
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowNetlify", policy =>
                {
                    policy
                        .WithOrigins(
                            "https://creativebudgeting.netlify.app", // Replace with your actual Netlify URL
                            "http://localhost:4200", // For local Angular development
                            "http://localhost:5000"  // For local testing
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
                
                // For development/testing
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });

            // Database connection - Render provides DATABASE_URL environment variable
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
                                   ?? builder.Configuration.GetConnectionString("DefaultConnection");
            
            // Configure Hangfire with PostgreSQL
            builder.Services.AddHangfire(config =>
            {
                config.UsePostgreSqlStorage(connectionString);
            });
            builder.Services.AddHangfireServer();

            // Configure Entity Framework with PostgreSQL
            builder.Services.AddDbContext<BudgetDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Register services
            builder.Services.AddScoped<PasswordService>();
            builder.Services.AddScoped<GlobalMethodService>();
            
            // Email service registration
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddScoped<IEmailService, MockEmailService>();
            }
            else
            {
                builder.Services.AddScoped<IEmailService, EmailService>();
            }
            
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseHangfireDashboard();
                app.UseCors("AllowAll");
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseCors("AllowNetlify");
            }

            // For Render, we might not always want to force HTTPS redirect
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseSession();
            app.UseRouting();
            app.MapControllers();

            // Auto-migrate database on startup
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var context = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();
                    await context.Database.MigrateAsync();

                    // Initialize Hangfire jobs
                    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
                    var globalMethodService = scope.ServiceProvider.GetRequiredService<GlobalMethodService>();

                    recurringJobManager.AddOrUpdate(
                        "MarkExpensesUnpaidMonthly",
                        () => globalMethodService.MarkAllExpensesUnpaidAsync(),
                        "0 0 1 * *"
                    );
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "Failed to initialize database or Hangfire jobs");
                }
            }

            await app.RunAsync();
        }
    }
}
