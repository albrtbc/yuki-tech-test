using FluentAssertions;
using Xunit;
using Yuki.Blog.Application.Common.Models;

namespace Yuki.Blog.Application.UnitTests.Common.Models;

public class ErrorTests
{
    [Fact]
    public void Validation_ShouldCreateValidationError()
    {
        // Arrange
        var message = "Validation failed";

        // Act
        var error = Error.Validation(message);

        // Assert
        error.Type.Should().Be(ErrorType.Validation);
        error.Message.Should().Be(message);
    }

    [Fact]
    public void NotFound_ShouldCreateNotFoundError()
    {
        // Arrange
        var message = "Resource not found";

        // Act
        var error = Error.NotFound(message);

        // Assert
        error.Type.Should().Be(ErrorType.NotFound);
        error.Message.Should().Be(message);
    }

    [Fact]
    public void Conflict_ShouldCreateConflictError()
    {
        // Arrange
        var message = "Resource conflict";

        // Act
        var error = Error.Conflict(message);

        // Assert
        error.Type.Should().Be(ErrorType.Conflict);
        error.Message.Should().Be(message);
    }

    [Fact]
    public void Unauthorized_ShouldCreateUnauthorizedError()
    {
        // Arrange
        var message = "Unauthorized access";

        // Act
        var error = Error.Unauthorized(message);

        // Assert
        error.Type.Should().Be(ErrorType.Unauthorized);
        error.Message.Should().Be(message);
    }

    [Fact]
    public void Forbidden_ShouldCreateForbiddenError()
    {
        // Arrange
        var message = "Access forbidden";

        // Act
        var error = Error.Forbidden(message);

        // Assert
        error.Type.Should().Be(ErrorType.Forbidden);
        error.Message.Should().Be(message);
    }

    [Fact]
    public void Internal_ShouldCreateInternalError()
    {
        // Arrange
        var message = "Internal server error";

        // Act
        var error = Error.Internal(message);

        // Assert
        error.Type.Should().Be(ErrorType.Internal);
        error.Message.Should().Be(message);
    }

    [Fact]
    public void None_ShouldHaveEmptyMessageAndValidationType()
    {
        // Act
        var error = Error.None;

        // Assert
        error.Type.Should().Be(ErrorType.Validation);
        error.Message.Should().BeEmpty();
    }

    [Fact]
    public void ErrorType_ShouldHaveAllDefinedValues()
    {
        // Assert - Ensure all enum values are present
        Enum.GetValues<ErrorType>().Should().Contain(new[]
        {
            ErrorType.Validation,
            ErrorType.NotFound,
            ErrorType.Conflict,
            ErrorType.Unauthorized,
            ErrorType.Forbidden,
            ErrorType.Internal
        });
    }

    [Fact]
    public void Errors_WithSameTypeAndMessage_ShouldNotBeSameInstance()
    {
        // Arrange
        var message = "Test message";

        // Act
        var error1 = Error.Validation(message);
        var error2 = Error.Validation(message);

        // Assert
        error1.Should().NotBeSameAs(error2);
        error1.Type.Should().Be(error2.Type);
        error1.Message.Should().Be(error2.Message);
    }

    [Fact]
    public void None_ShouldBeSingletonInstance()
    {
        // Act
        var none1 = Error.None;
        var none2 = Error.None;

        // Assert
        none1.Should().BeSameAs(none2);
    }
}
