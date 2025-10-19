using FluentAssertions;
using Xunit;
using Yuki.Blog.Application.Common.Models;

namespace Yuki.Blog.Application.UnitTests.Common.Models;

public class ApplicationResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessResult()
    {
        // Arrange
        var value = "Test Value";

        // Act
        var result = ApplicationResult<string>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailureResult()
    {
        // Arrange
        var error = Error.NotFound("Resource not found");

        // Act
        var result = ApplicationResult<string>.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Constructor_WithSuccessAndError_ShouldThrowException()
    {
        // Arrange
        var value = "Test Value";
        var error = Error.NotFound("Error");

        // Act & Assert
        var act = () => new ApplicationResult<string>(true, value, error);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("A successful result cannot have an error.");
    }

    [Fact]
    public void Constructor_WithFailureAndNoError_ShouldThrowException()
    {
        // Arrange
        var value = "Test Value";

        // Act & Assert
        var act = () => new ApplicationResult<string>(false, value, Error.None);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("A failed result must have an error.");
    }

    [Fact]
    public void NonGenericSuccess_ShouldCreateSuccessResult()
    {
        // Act
        var result = ApplicationResult.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void NonGenericFailure_ShouldCreateFailureResult()
    {
        // Arrange
        var error = Error.Validation("Validation failed");

        // Act
        var result = ApplicationResult.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Success_WithNullValue_ShouldWork()
    {
        // Act
        var result = ApplicationResult<string?>.Success(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void Success_WithComplexObject_ShouldWork()
    {
        // Arrange
        var complexObject = new { Id = 1, Name = "Test" };

        // Act
        var result = ApplicationResult<object>.Success(complexObject);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(complexObject);
    }

    [Fact]
    public void IsFailure_ShouldBeOppositeOfIsSuccess()
    {
        // Arrange
        var successResult = ApplicationResult<string>.Success("value");
        var failureResult = ApplicationResult<string>.Failure(Error.NotFound("error"));

        // Assert
        successResult.IsFailure.Should().Be(!successResult.IsSuccess);
        failureResult.IsFailure.Should().Be(!failureResult.IsSuccess);
    }
}
