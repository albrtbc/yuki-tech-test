namespace Yuki.Blog.Application.Common.Models;

/// <summary>
/// Represents the type of error for HTTP status mapping.
/// </summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    Internal
}

/// <summary>
/// Represents an application error with a type and message.
/// Simplified design - no redundant codes or nested classes.
/// </summary>
public sealed class Error
{
    /// <summary>
    /// The type of error (maps to HTTP status codes).
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// A human-readable message describing the error.
    /// </summary>
    public string Message { get; }

    private Error(ErrorType type, string message)
    {
        Type = type;
        Message = message;
    }

    /// <summary>
    /// Creates a validation error (400 Bad Request).
    /// </summary>
    public static Error Validation(string message) => new(ErrorType.Validation, message);

    /// <summary>
    /// Creates a not found error (404 Not Found).
    /// </summary>
    public static Error NotFound(string message) => new(ErrorType.NotFound, message);

    /// <summary>
    /// Creates a conflict error (409 Conflict).
    /// </summary>
    public static Error Conflict(string message) => new(ErrorType.Conflict, message);

    /// <summary>
    /// Creates an unauthorized error (401 Unauthorized).
    /// </summary>
    public static Error Unauthorized(string message) => new(ErrorType.Unauthorized, message);

    /// <summary>
    /// Creates a forbidden error (403 Forbidden).
    /// </summary>
    public static Error Forbidden(string message) => new(ErrorType.Forbidden, message);

    /// <summary>
    /// Creates an internal server error (500 Internal Server Error).
    /// </summary>
    public static Error Internal(string message) => new(ErrorType.Internal, message);

    /// <summary>
    /// Represents the absence of an error (success case).
    /// </summary>
    public static readonly Error None = new(ErrorType.Validation, string.Empty);
}
