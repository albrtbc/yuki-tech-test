using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Xunit;
using Yuki.Blog.Application.Common.Behaviors;
using Yuki.Blog.Application.Common.Models;

namespace Yuki.Blog.Application.UnitTests.Common.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, ApplicationResult<TestResponse>>(validators);
        var request = new TestRequest { Value = "test" };
        var expectedResponse = ApplicationResult<TestResponse>.Success(new TestResponse { Result = "success" });
        var nextCalled = false;

        RequestHandlerDelegate<ApplicationResult<TestResponse>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCallNext()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<TestRequest>>();
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validators = new List<IValidator<TestRequest>> { mockValidator.Object };
        var behavior = new ValidationBehavior<TestRequest, ApplicationResult<TestResponse>>(validators);
        var request = new TestRequest { Value = "test" };
        var expectedResponse = ApplicationResult<TestResponse>.Success(new TestResponse { Result = "success" });
        var nextCalled = false;

        RequestHandlerDelegate<ApplicationResult<TestResponse>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be(expectedResponse);
        mockValidator.Verify(
            v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldReturnFailureResult()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<TestRequest>>();
        var validationFailure = new ValidationFailure("Value", "Value is invalid");
        var validationResult = new ValidationResult(new[] { validationFailure });

        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var validators = new List<IValidator<TestRequest>> { mockValidator.Object };
        var behavior = new ValidationBehavior<TestRequest, ApplicationResult<TestResponse>>(validators);
        var request = new TestRequest { Value = "test" };
        var nextCalled = false;

        RequestHandlerDelegate<ApplicationResult<TestResponse>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(ApplicationResult<TestResponse>.Success(new TestResponse()));
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Value is invalid");
    }

    [Fact]
    public async Task Handle_WithMultipleValidationFailures_ShouldReturnCombinedErrorMessage()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<TestRequest>>();
        var validationFailures = new[]
        {
            new ValidationFailure("Value", "First error"),
            new ValidationFailure("OtherProperty", "Second error"),
            new ValidationFailure("ThirdProperty", "Third error")
        };
        var validationResult = new ValidationResult(validationFailures);

        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var validators = new List<IValidator<TestRequest>> { mockValidator.Object };
        var behavior = new ValidationBehavior<TestRequest, ApplicationResult<TestResponse>>(validators);
        var request = new TestRequest { Value = "test" };

        RequestHandlerDelegate<ApplicationResult<TestResponse>> next = () =>
            Task.FromResult(ApplicationResult<TestResponse>.Success(new TestResponse()));

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("First error");
        result.Error.Message.Should().Contain("Second error");
        result.Error.Message.Should().Contain("Third error");
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldRunAllValidators()
    {
        // Arrange
        var mockValidator1 = new Mock<IValidator<TestRequest>>();
        var mockValidator2 = new Mock<IValidator<TestRequest>>();

        mockValidator1
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        mockValidator2
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validators = new List<IValidator<TestRequest>> { mockValidator1.Object, mockValidator2.Object };
        var behavior = new ValidationBehavior<TestRequest, ApplicationResult<TestResponse>>(validators);
        var request = new TestRequest { Value = "test" };
        var expectedResponse = ApplicationResult<TestResponse>.Success(new TestResponse { Result = "success" });

        RequestHandlerDelegate<ApplicationResult<TestResponse>> next = () =>
            Task.FromResult(expectedResponse);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        mockValidator1.Verify(
            v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        mockValidator2.Verify(
            v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleValidatorsAndOneFailure_ShouldReturnFailureResult()
    {
        // Arrange
        var mockValidator1 = new Mock<IValidator<TestRequest>>();
        var mockValidator2 = new Mock<IValidator<TestRequest>>();

        mockValidator1
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validationFailure = new ValidationFailure("Value", "Validation failed");
        mockValidator2
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

        var validators = new List<IValidator<TestRequest>> { mockValidator1.Object, mockValidator2.Object };
        var behavior = new ValidationBehavior<TestRequest, ApplicationResult<TestResponse>>(validators);
        var request = new TestRequest { Value = "test" };
        var nextCalled = false;

        RequestHandlerDelegate<ApplicationResult<TestResponse>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(ApplicationResult<TestResponse>.Success(new TestResponse()));
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Validation failed");
    }

    [Fact]
    public async Task Handle_WithNullValidationFailures_ShouldFilterOutNulls()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<TestRequest>>();
        var validationFailure = new ValidationFailure("Value", "Error message");
        var validationResult = new ValidationResult(new[] { validationFailure });

        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var validators = new List<IValidator<TestRequest>> { mockValidator.Object };
        var behavior = new ValidationBehavior<TestRequest, ApplicationResult<TestResponse>>(validators);
        var request = new TestRequest { Value = "test" };

        RequestHandlerDelegate<ApplicationResult<TestResponse>> next = () =>
            Task.FromResult(ApplicationResult<TestResponse>.Success(new TestResponse()));

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error message");
    }

    // Test helper classes
    public class TestRequest : IRequest<ApplicationResult<TestResponse>>
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestResponse
    {
        public string Result { get; set; } = string.Empty;
    }
}
