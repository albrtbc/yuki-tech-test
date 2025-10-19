namespace Yuki.Blog.Application.Common.Interfaces;

/// <summary>
/// Abstraction for DateTime to make time-dependent code testable.
/// </summary>
public interface IDateTime
{
    /// <summary>
    /// Gets the current date and time in UTC.
    /// </summary>
    DateTime UtcNow { get; }
}
