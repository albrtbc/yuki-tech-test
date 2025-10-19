using FluentAssertions;
using Xunit;
using Yuki.Blog.Domain.Common;

namespace Yuki.Blog.Domain.UnitTests.Common;

public class DomainResultTests
{
    [Fact]
    public void GenericSuccess_ShouldCreateSuccessResult()
    {
        // Arrange
        var value = "Test Value";

        // Act
        var result = DomainResult<string>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void GenericFailure_ShouldCreateFailureResult()
    {
        // Arrange
        var errorMessage = "Something went wrong";

        // Act
        var result = DomainResult<string>.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be(errorMessage);
        result.Value.Should().BeNull();
    }

    [Fact]
    public void GenericImplicitConversion_FromValue_ShouldCreateSuccessResult()
    {
        // Arrange
        var value = 42;

        // Act
        DomainResult<int> result = value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void NonGenericSuccess_ShouldCreateSuccessResult()
    {
        // Act
        var result = DomainResult.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void NonGenericFailure_ShouldCreateFailureResult()
    {
        // Arrange
        var errorMessage = "Operation failed";

        // Act
        var result = DomainResult.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void GenericSuccess_WithComplexType_ShouldWork()
    {
        // Arrange
        var complexValue = new { Name = "Test", Value = 123 };

        // Act
        var result = DomainResult<object>.Success(complexValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(complexValue);
    }

    [Fact]
    public void GenericFailure_WithEmptyErrorMessage_ShouldStillBeFailure()
    {
        // Act
        var result = DomainResult<string>.Failure(string.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void IsFailure_ShouldBeOppositeOfIsSuccess()
    {
        // Arrange & Act
        var successResult = DomainResult<int>.Success(42);
        var failureResult = DomainResult<int>.Failure("Error");

        // Assert
        successResult.IsFailure.Should().Be(!successResult.IsSuccess);
        failureResult.IsFailure.Should().Be(!failureResult.IsSuccess);
    }

    [Fact]
    public void NonGenericIsFailure_ShouldBeOppositeOfIsSuccess()
    {
        // Arrange & Act
        var successResult = DomainResult.Success();
        var failureResult = DomainResult.Failure("Error");

        // Assert
        successResult.IsFailure.Should().Be(!successResult.IsSuccess);
        failureResult.IsFailure.Should().Be(!failureResult.IsSuccess);
    }

    [Theory]
    [InlineData("Error message 1")]
    [InlineData("Another error")]
    [InlineData("Validation failed")]
    public void GenericFailure_WithDifferentMessages_ShouldPreserveMessage(string errorMessage)
    {
        // Act
        var result = DomainResult<string>.Failure(errorMessage);

        // Assert
        result.ErrorMessage.Should().Be(errorMessage);
    }

    [Theory]
    [InlineData("Error message 1")]
    [InlineData("Another error")]
    [InlineData("Validation failed")]
    public void NonGenericFailure_WithDifferentMessages_ShouldPreserveMessage(string errorMessage)
    {
        // Act
        var result = DomainResult.Failure(errorMessage);

        // Assert
        result.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void GenericSuccess_ErrorMessage_ShouldBeEmpty()
    {
        // Act
        var result = DomainResult<string>.Success("test");

        // Assert
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void NonGenericSuccess_ErrorMessage_ShouldBeEmpty()
    {
        // Act
        var result = DomainResult.Success();

        // Assert
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void GenericFailure_Value_ShouldBeDefault()
    {
        // Act
        var result = DomainResult<string>.Failure("Error");

        // Assert
        result.Value.Should().BeNull();
    }
}
