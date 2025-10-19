using Yuki.Blog.Application.Common.Interfaces;

namespace Yuki.Blog.Infrastructure.Services;

/// <summary>
/// Implementation of IDateTime for production use.
/// Returns the actual current time.
/// </summary>
public class DateTimeService : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}
