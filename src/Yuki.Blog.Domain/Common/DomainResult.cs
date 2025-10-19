namespace Yuki.Blog.Domain.Common;

/// <summary>
/// Represents the result of a domain operation that can succeed or fail.
/// Used to avoid exceptions for expected validation and business rule failures.
/// Domain layer uses simple string error messages.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class DomainResult<T>
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The value returned on success. Only access if IsSuccess is true.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// The error message if the operation failed.
    /// </summary>
    public string ErrorMessage { get; }

    private DomainResult(bool isSuccess, T value, string errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    public static DomainResult<T> Success(T value) => new(true, value, string.Empty);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    public static DomainResult<T> Failure(string errorMessage) => new(false, default!, errorMessage);

    /// <summary>
    /// Implicit conversion from value to successful result.
    /// </summary>
    public static implicit operator DomainResult<T>(T value) => Success(value);
}

/// <summary>
/// Non-generic result for operations that don't return a value.
/// </summary>
public class DomainResult
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The error message if the operation failed.
    /// </summary>
    public string ErrorMessage { get; }

    private DomainResult(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static DomainResult Success() => new(true, string.Empty);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    public static DomainResult Failure(string errorMessage) => new(false, errorMessage);
}
