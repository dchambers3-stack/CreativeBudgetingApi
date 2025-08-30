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
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

            // If DATABASE_URL is not set, fall back to configuration
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            }

            // Convert Render's DATABASE_URL format to Npgsql connection string if needed
            if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgres://"))
            {
                // Render uses postgres:// but Npgsql expects postgresql://
                connectionString = connectionString.Replace("postgres://", "postgresql://");
            }

            Console.WriteLine($"Using connection string: {connectionString?.Substring(0, Math.Min(50, connectionString.Length ?? 0))}...");

            // Configure Hangfire with PostgreSQL
            builder.Services.AddHangfire(config =>
            {
                if (!string.IsNullOrEmpty(connectionString))
                {
                    config.UsePostgreSqlStorage(connectionString);
                }
            });
            builder.Services.AddHangfireServer();

            // Configure Entity Framework with PostgreSQL
            builder.Services.AddDbContext<BudgetDbContext>(options =>
            {
                if (!string.IsNullOrEmpty(connectionString))
                {
                    options.UseNpgsql(connectionString);
                }
                else
                {
                    throw new InvalidOperationException("No database connection string found. Please set DATABASE_URL environment variable or DefaultConnection in appsettings.json");
                }
            });

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
                    
                    // Test database connection first
                    var canConnect = await context.Database.CanConnectAsync();
                    if (!canConnect)
                    {
                        app.Logger.LogError("Cannot connect to database. Check DATABASE_URL environment variable.");
                        throw new InvalidOperationException("Database connection failed");
                    }
                    
                    app.Logger.LogInformation("Database connection successful. Running migrations...");
                    await context.Database.MigrateAsync();
                    app.Logger.LogInformation("Database migrations completed successfully.");

                    // Initialize Hangfire jobs
                    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
                    var globalMethodService = scope.ServiceProvider.GetRequiredService<GlobalMethodService>();

                    recurringJobManager.AddOrUpdate(
                        "MarkExpensesUnpaidMonthly",
                        () => globalMethodService.MarkAllExpensesUnpaidAsync(),
                        "0 0 1 * *"
                    );
                    
                    app.Logger.LogInformation("Hangfire jobs initialized successfully.");
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "Failed to initialize database or Hangfire jobs. Error: {ErrorMessage}", ex.Message);
                    
                    // Log connection string info (without sensitive data)
                    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                    if (string.IsNullOrEmpty(dbUrl))
                    {
                        app.Logger.LogError("DATABASE_URL environment variable is not set!");
                    }
                    else
                    {
                        app.Logger.LogInformation("DATABASE_URL is set (length: {Length})", dbUrl.Length);
                    }
                    
                    // Don't exit the app, but log the error
                }
            }

            await app.RunAsync();
        }
    }
}
