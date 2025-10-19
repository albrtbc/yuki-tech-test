using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yuki.Blog.Application.Common.Interfaces;
using Yuki.Blog.Domain.ReadOnlyRepositories;
using Yuki.Blog.Domain.Repositories;
using Yuki.Blog.Infrastructure.Persistence;
using Yuki.Blog.Infrastructure.Persistence.ReadOnlyRepositories;
using Yuki.Blog.Infrastructure.Persistence.Repositories;
using Yuki.Blog.Infrastructure.Services;

namespace Yuki.Blog.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Infrastructure layer services into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<BlogDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(BlogDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                });

            // Enable sensitive data logging in Development
            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
        });

        // Register Unit of Work (BlogDbContext implements IUnitOfWork)
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<BlogDbContext>());

        // Register command repositories (write operations)
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IAuthorRepository, AuthorRepository>();

        // Register read-only repositories (read operations)
        services.AddScoped<IPostReadOnlyRepository, PostReadOnlyRepository>();
        services.AddScoped<IAuthorReadOnlyRepository, AuthorReadOnlyRepository>();

        // Register services
        services.AddSingleton<IDateTime, DateTimeService>();

        return services;
    }
}
