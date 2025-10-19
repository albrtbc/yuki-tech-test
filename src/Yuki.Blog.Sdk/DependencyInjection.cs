using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Yuki.Blog.Sdk.Configuration;
using Yuki.Blog.Sdk.Interfaces;

namespace Yuki.Blog.Sdk;

/// <summary>
/// Extension methods for registering the Blog SDK.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the Blog SDK to the service collection with configuration from appsettings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// // In appsettings.json:
    /// // {
    /// //   "BlogClient": {
    /// //     "BaseUrl": "https://api.yourdomain.com",
    /// //     "ApiVersion": "1.0"
    /// //   }
    /// // }
    ///
    /// services.AddBlogClient(configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddBlogClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BlogClientOptions>(
            configuration.GetSection(BlogClientOptions.SectionName));

        return AddBlogClientCore(services);
    }

    /// <summary>
    /// Adds the Blog SDK to the service collection with manual configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure the client options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddBlogClient(options =>
    /// {
    ///     options.BaseUrl = "https://api.yourdomain.com";
    ///     options.ApiVersion = "1.0";
    ///     options.TimeoutSeconds = 30;
    ///     options.EnableRetry = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddBlogClient(
        this IServiceCollection services,
        Action<BlogClientOptions> configure)
    {
        services.Configure(configure);

        return AddBlogClientCore(services);
    }

    /// <summary>
    /// Core registration logic for the Blog SDK.
    /// </summary>
    private static IServiceCollection AddBlogClientCore(IServiceCollection services)
    {
        services.AddHttpClient<IBlogClient, BlogClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<
                    Microsoft.Extensions.Options.IOptions<BlogClientOptions>>().Value;

                if (string.IsNullOrEmpty(options.BaseUrl))
                {
                    throw new InvalidOperationException(
                        "BlogClient BaseUrl is required. " +
                        "Configure it in appsettings.json or via AddBlogClient configuration.");
                }

                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            // Add retry policy (if enabled)
            .AddPolicyHandler((serviceProvider, request) =>
            {
                var options = serviceProvider.GetRequiredService<
                    Microsoft.Extensions.Options.IOptions<BlogClientOptions>>().Value;

                if (!options.EnableRetry)
                {
                    return Policy.NoOpAsync<HttpResponseMessage>();
                }

                // Retry policy for transient errors
                return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(
                        options.MaxRetryAttempts,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (outcome, timespan, retryAttempt, context) => {});
            });

        // Note: No custom correlation/tracing handler needed!
        // When the consuming application has OpenTelemetry configured with
        // .AddHttpClientInstrumentation(), the W3C Trace Context (traceparent header)
        // is automatically propagated by .NET's HttpClient. Zero configuration required.

        return services;
    }
}
