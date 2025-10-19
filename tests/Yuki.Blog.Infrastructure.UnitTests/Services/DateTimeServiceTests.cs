using FluentAssertions;
using Yuki.Blog.Infrastructure.Services;

namespace Yuki.Blog.Infrastructure.UnitTests.Services;

public class DateTimeServiceTests
{
    [Fact]
    public void UtcNow_ShouldReturnCurrentUtcTime()
    {
        // Arrange
        var service = new DateTimeService();
        var before = DateTime.UtcNow;

        // Act
        var result = service.UtcNow;

        // Assert
        var after = DateTime.UtcNow;
        result.Should().BeOnOrAfter(before);
        result.Should().BeOnOrBefore(after);
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void UtcNow_CalledMultipleTimes_ShouldReturnDifferentOrEqualTimes()
    {
        // Arrange
        var service = new DateTimeService();

        // Act
        var time1 = service.UtcNow;
        Thread.Sleep(1); // Small delay to potentially get different time
        var time2 = service.UtcNow;

        // Assert
        time2.Should().BeOnOrAfter(time1);
    }
}
