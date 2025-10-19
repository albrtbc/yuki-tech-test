namespace Yuki.Blog.Sdk.Configuration;

/// <summary>
/// Configuration options for the Blog SDK client.
/// </summary>
public class BlogClientOptions
{
    /// <summary>
    /// Configuration section name for appsettings.json binding.
    /// </summary>
    public const string SectionName = "BlogClient";

    /// <summary>
    /// The base URL of the Blog API (e.g., https://api.yourdomain.com).
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// API version to use (default: 1.0).
    /// </summary>
    public string ApiVersion { get; set; } = "1.0";

    /// <summary>
    /// Request timeout in seconds (default: 30).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable automatic retry on transient failures (default: true).
    /// </summary>
    public bool EnableRetry { get; set; } = true;

    /// <summary>
    /// Maximum number of retry attempts (default: 3).
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
}
