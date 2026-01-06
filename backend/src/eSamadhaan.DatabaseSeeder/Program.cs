using eSamadhaan.Application.Interfaces.Services;
using eSamadhaan.Application.Services;
using eSamadhaan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace eSamadhaan.DatabaseSeeder;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("eSamadhaan Database Seeder");
        Console.WriteLine("==========================");
        Console.WriteLine();

        // Build configuration - try current directory first, then parent API directory
        var currentDir = Directory.GetCurrentDirectory();
        var apiDir = Path.GetFullPath(Path.Combine(currentDir, "..", "eSamadhaan.API"));
        
        var configBuilder = new ConfigurationBuilder();
        
        // Add appsettings from current directory (seeder project)
        configBuilder
            .SetBasePath(currentDir)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
        
        // Also try API project directory for appsettings (higher priority)
        if (Directory.Exists(apiDir))
        {
            configBuilder
                .SetBasePath(apiDir)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
        }
        
        var configuration = configBuilder
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        // Setup logging
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Information);
        });
        var logger = loggerFactory.CreateLogger<Program>();

        // Setup database context
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            logger.LogError("Connection string 'DefaultConnection' not found in configuration.");
            Console.WriteLine("ERROR: Connection string not configured.");
            Console.WriteLine("Please ensure appsettings.json contains a valid 'DefaultConnection' connection string.");
            Environment.Exit(1);
            return;
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        ));

        using var context = new ApplicationDbContext(optionsBuilder.Options);
        var passwordHasher = new PasswordHasher();
        var seederLogger = loggerFactory.CreateLogger<DataSeeder>();
        var seeder = new DataSeeder(context, passwordHasher, seederLogger);

        try
        {
            Console.WriteLine("Connecting to database...");
            await context.Database.CanConnectAsync();
            Console.WriteLine("Database connection successful.");
            Console.WriteLine();

            Console.WriteLine("Starting seed operation...");
            await seeder.SeedAsync();
            Console.WriteLine();
            Console.WriteLine("✓ Database seeding completed successfully!");
            Console.WriteLine();
            Console.WriteLine("Default login credentials:");
            Console.WriteLine("  Admin: admin@esamadhaan.test / Password123!");
            Console.WriteLine("  Supervisor: supervisor.pwd@esamadhaan.test / Password123!");
            Console.WriteLine("  Officer: officer.pwd1@esamadhaan.test / Password123!");
            Console.WriteLine("  Citizen: citizen.ramesh@test.com / Password123!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during database seeding.");
            Console.WriteLine();
            Console.WriteLine($"ERROR: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            Environment.Exit(1);
        }
    }
}

