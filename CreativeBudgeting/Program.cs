using System.Text.Json;
using CreativeBudgeting.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;

namespace CreativeBudgeting
{
    public class Program
    {
        private static string ConvertPostgresUrlToConnectionString(string databaseUrl)
        {
            try
            {
                var uri = new Uri(databaseUrl);
                var host = uri.Host;
                var port = uri.Port > 0 ? uri.Port : 5432;
                var database = uri.LocalPath.TrimStart('/');
                var username = uri.UserInfo.Split(':')[0];
                var password = uri.UserInfo.Split(':')[1];

                return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse DATABASE_URL: {ex.Message}");
                throw new ArgumentException($"Invalid DATABASE_URL format: {ex.Message}", ex);
            }
        }

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Kestrel for Render
            builder.WebHost.ConfigureKestrel(options =>
            {
                var portString = Environment.GetEnvironmentVariable("PORT") ?? "10000";
                var port = int.Parse(portString);
                options.ListenAnyIP(port);
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
            
            // Debug logging - Force rebuild for Render
            Console.WriteLine($"DATABASE_URL environment variable: {(string.IsNullOrEmpty(connectionString) ? "NOT SET" : "SET")}");
            Console.WriteLine("=== USING NEW CONNECTION STRING PARSING CODE ===");
            
            // If DATABASE_URL is not set, fall back to configuration
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                Console.WriteLine($"Using DefaultConnection from config: {(string.IsNullOrEmpty(connectionString) ? "NOT FOUND" : "FOUND")}");
            }

            // Parse and convert Render's DATABASE_URL to proper Npgsql format
            if (!string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    Console.WriteLine($"Raw connection string: {connectionString.Substring(0, Math.Min(60, connectionString.Length))}...");
                    
                    // If it's a Render-style URL, convert it to Npgsql format
                    if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
                    {
                        Console.WriteLine("Converting URL-style connection string to Npgsql format...");
                        connectionString = ConvertPostgresUrlToConnectionString(connectionString);
                        Console.WriteLine($"Converted connection string: {connectionString.Substring(0, Math.Min(60, connectionString.Length))}...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing connection string: {ex.Message}");
                    throw;
                }
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("ERROR: No connection string available!");
            }
            else
            {
                Console.WriteLine($"Final connection string format validated successfully.");
            }

            // Configure Hangfire with PostgreSQL
            if (!string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    Console.WriteLine("Configuring Hangfire with PostgreSQL...");
                    builder.Services.AddHangfire(config =>
                    {
                        config.UsePostgreSqlStorage(connectionString);
                    });
                    builder.Services.AddHangfireServer();
                    Console.WriteLine("Hangfire configuration completed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR configuring Hangfire: {ex.Message}");
                    throw;
                }
            }
            else
            {
                Console.WriteLine("Skipping Hangfire configuration - no connection string");
            }

            // Configure Entity Framework with PostgreSQL
            builder.Services.AddDbContext<BudgetDbContext>(options =>
            {
                if (!string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("Configuring Entity Framework with PostgreSQL...");
                    options.UseNpgsql(connectionString);
                    Console.WriteLine("Entity Framework configuration completed.");
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
                // Don't use HTTPS redirect on Render - they handle SSL termination
                // app.UseHttpsRedirection();
            }

            app.UseSession();
            app.UseRouting();
            app.MapControllers();

            // Auto-migrate database on startup
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    Console.WriteLine("Starting database initialization...");
                    var context = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();
                    
                    Console.WriteLine("Testing database connection...");
                    // Test database connection first
                    var canConnect = await context.Database.CanConnectAsync();
                    if (!canConnect)
                    {
                        app.Logger.LogError("Cannot connect to database. Check DATABASE_URL environment variable.");
                        Console.WriteLine("Database connection test FAILED");
                        throw new InvalidOperationException("Database connection failed");
                    }
                    
                    Console.WriteLine("Database connection test PASSED");
                    app.Logger.LogInformation("Database connection successful. Running migrations...");
                    await context.Database.MigrateAsync();
                    app.Logger.LogInformation("Database migrations completed successfully.");

                    Console.WriteLine("Starting Hangfire job initialization...");
                    // Initialize Hangfire jobs
                    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
                    var globalMethodService = scope.ServiceProvider.GetRequiredService<GlobalMethodService>();

                    recurringJobManager.AddOrUpdate(
                        "MarkExpensesUnpaidMonthly",
                        () => globalMethodService.MarkAllExpensesUnpaidAsync(),
                        "0 0 1 * *"
                    );
                    
                    Console.WriteLine("Hangfire job initialization completed successfully.");
                    app.Logger.LogInformation("Hangfire jobs initialized successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database/Hangfire initialization failed: {ex.Message}");
                    Console.WriteLine($"Exception type: {ex.GetType().Name}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    
                    app.Logger.LogError(ex, "Failed to initialize database or Hangfire jobs. Error: {ErrorMessage}", ex.Message);
                    
                    // Log connection string info (without sensitive data)
                    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                    if (string.IsNullOrEmpty(dbUrl))
                    {
                        app.Logger.LogError("DATABASE_URL environment variable is not set!");
                        Console.WriteLine("DATABASE_URL environment variable is NOT SET!");
                    }
                    else
                    {
                        app.Logger.LogInformation("DATABASE_URL is set (length: {Length})", dbUrl.Length);
                        Console.WriteLine($"DATABASE_URL is set (length: {dbUrl.Length})");
                    }
                    
                    // Don't exit the app, but log the error
                }
            }

            await app.RunAsync();
        }
    }
}
