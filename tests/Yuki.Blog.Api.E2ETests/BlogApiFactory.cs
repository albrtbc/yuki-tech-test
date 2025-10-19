using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;
using Yuki.Blog.Infrastructure.Persistence;

namespace Yuki.Blog.Api.E2ETests;

/// <summary>
/// Custom WebApplicationFactory for E2E testing with Testcontainers PostgreSQL.
/// This factory sets up an isolated test environment with a real PostgreSQL database.
/// </summary>
public class BlogApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? _dbContainer;

    private PostgreSqlContainer DbContainer
    {
        get
        {
            if (_dbContainer == null)
            {
                _dbContainer = new PostgreSqlBuilder()
                    .WithImage("postgres:16-alpine")
                    .WithDatabase("blogdb_test")
                    .WithUsername("testuser")
                    .WithPassword("testpass")
                    .Build();

                // Start the container synchronously in constructor
                _dbContainer.StartAsync().GetAwaiter().GetResult();
            }
            return _dbContainer;
        }
    }

    /// <summary>
    /// Configures the web host to use the test database container.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to prevent appsettings.json from loading Serilog
        builder.UseEnvironment("Testing");

        // Override configuration to use test container connection string
        // DbContainer getter will start the container if not already started
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add in-memory configuration with highest priority (added last wins)
            var connectionString = DbContainer.GetConnectionString();
            var configValues = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            };
            config.AddInMemoryCollection(configValues);
        });

        // Completely suppress all logging during tests
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders(); // Remove all logging providers (including Serilog)
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove the existing DbContext configuration
            services.RemoveAll<DbContextOptions<BlogDbContext>>();
            services.RemoveAll<BlogDbContext>();

            // Add DbContext with test container connection string
            services.AddDbContext<BlogDbContext>(options =>
            {
                options.UseNpgsql(DbContainer.GetConnectionString());
                // Disable EF Core logging completely
            });

            // Remove all existing health check registrations
            // Remove both IHealthCheck implementations and HealthCheckRegistration options
            var healthCheckDescriptors = services
                .Where(d => d.ServiceType == typeof(IHealthCheck) ||
                           d.ServiceType.Name.Contains("HealthCheckRegistration"))
                .ToList();
            foreach (var descriptor in healthCheckDescriptors)
            {
                services.Remove(descriptor);
            }

            // Also remove IOptions<HealthCheckServiceOptions> if it exists
            services.Configure<HealthCheckServiceOptions>(options =>
            {
                options.Registrations.Clear();
            });

            // Re-add health checks with test container connection string
            services.AddHealthChecks()
                .AddNpgSql(DbContainer.GetConnectionString());
        });
    }

    /// <summary>
    /// Initializes the test environment and applies migrations.
    /// </summary>
    public async Task InitializeAsync()
    {
        // DbContainer getter ensures container is started
        _ = DbContainer;

        // Force build the services to ensure the WebHost is created with our test configuration
        _ = Services;

        // Apply migrations after container is started
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();

        // Ensure database is created and apply migrations
        await dbContext.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Stops and disposes the PostgreSQL container after tests complete.
    /// </summary>
    public new async Task DisposeAsync()
    {
        if (_dbContainer != null)
        {
            await _dbContainer.StopAsync();
            await _dbContainer.DisposeAsync();
        }
    }
}
