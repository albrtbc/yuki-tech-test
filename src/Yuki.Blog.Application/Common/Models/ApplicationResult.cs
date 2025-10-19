namespace Yuki.Blog.Application.Common.Models;

/// <summary>
/// Represents the result of an application-layer operation that can succeed or fail.
/// Implements the Result pattern to avoid exceptions for flow control.
/// Uses rich Error types with HTTP status code mappings.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class ApplicationResult<T>
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
    /// The value returned on success. Throws if accessed on failure.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// The error that occurred on failure.
    /// </summary>
    public Error Error { get; }

    public ApplicationResult(bool isSuccess, T value, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("A successful result cannot have an error.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("A failed result must have an error.");
        }

        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    public static ApplicationResult<T> Success(T value) => new(true, value, Error.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static ApplicationResult<T> Failure(Error error) => new(false, default!, error);
}

/// <summary>
/// Non-generic result for operations that don't return a value.
/// </summary>
public class ApplicationResult : ApplicationResult<object>
{
    private ApplicationResult(bool isSuccess, Error error) : base(isSuccess, null!, error)
    {
    }

    public static ApplicationResult Success() => new(true, Error.None);

    public static ApplicationResult Failure(Error error) => new(false, error);
}
