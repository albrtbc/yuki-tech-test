using System.Net;

namespace Yuki.Blog.Sdk.Exceptions;

/// <summary>
/// Base exception for Blog SDK errors.
/// </summary>
public class BlogApiException : Exception
{
    /// <summary>
    /// HTTP status code of the failed request (if applicable).
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Response content from the API (if available).
    /// </summary>
    public string? ResponseContent { get; }

    public BlogApiException(string message) : base(message)
    {
    }

    public BlogApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public BlogApiException(
        string message,
        HttpStatusCode statusCode,
        string? responseContent = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }

    public BlogApiException(
        string message,
        HttpStatusCode statusCode,
        string? responseContent,
        Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }
}

/// <summary>
/// Exception thrown when a resource is not found (HTTP 404).
/// </summary>
public class NotFoundException : BlogApiException
{
    public NotFoundException(string message)
        : base(message, HttpStatusCode.NotFound)
    {
    }

    public NotFoundException(string resourceType, Guid id)
        : base($"{resourceType} with ID '{id}' was not found.", HttpStatusCode.NotFound)
    {
    }
}

/// <summary>
/// Exception thrown when a request is invalid (HTTP 400).
/// </summary>
public class BadRequestException : BlogApiException
{
    public BadRequestException(string message, string? responseContent = null)
        : base(message, HttpStatusCode.BadRequest, responseContent)
    {
    }
}

/// <summary>
/// Exception thrown when rate limiting is applied (HTTP 429).
/// </summary>
public class RateLimitException : BlogApiException
{
    /// <summary>
    /// Time to wait before retrying (if provided by the API).
    /// </summary>
    public TimeSpan? RetryAfter { get; }

    public RateLimitException(string message, TimeSpan? retryAfter = null)
        : base(message, HttpStatusCode.TooManyRequests)
    {
        RetryAfter = retryAfter;
    }
}
