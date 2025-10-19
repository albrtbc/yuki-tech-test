using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;
using Yuki.Blog.Application.Common.Interfaces;
using Yuki.Blog.Application.Features.Posts.Commands.CreatePost;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.Repositories;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Application.UnitTests.Features.Posts.Commands.CreatePost;

public class CreatePostCommandHandlerTests
{
    private readonly Mock<IPostRepository> _mockPostRepository;
    private readonly Mock<IAuthorRepository> _mockAuthorRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IDateTime> _mockDateTime;
    private readonly CreatePostCommandHandler _handler;
    private readonly DateTime _testDateTime;

    public CreatePostCommandHandlerTests()
    {
        _mockPostRepository = new Mock<IPostRepository>();
        _mockAuthorRepository = new Mock<IAuthorRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockDateTime = new Mock<IDateTime>();
        _testDateTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        _mockDateTime.Setup(x => x.UtcNow).Returns(_testDateTime);

        _handler = new CreatePostCommandHandler(
            _mockPostRepository.Object,
            _mockAuthorRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockDateTime.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnSuccessResult()
    {
        // Arrange
        var authorId = AuthorId.CreateUnique();
        var command = new CreatePostCommand
        {
            AuthorId = authorId.Value,
            Title = "Test Post Title",
            Description = "Test post description",
            Content = "Test post content"
        };

        _mockAuthorRepository
            .Setup(x => x.ExistsAsync(It.IsAny<AuthorId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var expectedResponse = new CreatePostResponse
        {
            Id = Guid.NewGuid(),
            AuthorId = authorId.Value,
            Title = command.Title,
            Description = command.Description,
            Content = command.Content,
            CreatedAt = _testDateTime
        };

        _mockMapper
            .Setup(x => x.Map<CreatePostResponse>(It.IsAny<Post>()))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Title.Should().Be(command.Title);
        result.Value.Description.Should().Be(command.Description);
        result.Value.Content.Should().Be(command.Content);
        result.Value.AuthorId.Should().Be(command.AuthorId);
        result.Value.CreatedAt.Should().Be(_testDateTime);

        _mockPostRepository.Verify(
            x => x.AddAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentAuthor_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test Post Title",
            Description = "Test post description",
            Content = "Test post content"
        };

        _mockAuthorRepository
            .Setup(x => x.ExistsAsync(It.IsAny<AuthorId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Author");
        result.Error.Message.Should().Contain("not found");

        _mockPostRepository.Verify(
            x => x.AddAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidTitle_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "", // Invalid empty title
            Description = "Test post description",
            Content = "Test post content"
        };

        _mockAuthorRepository
            .Setup(x => x.ExistsAsync(It.IsAny<AuthorId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("title");

        _mockPostRepository.Verify(
            x => x.AddAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidDescription_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test Post Title",
            Description = "", // Invalid empty description
            Content = "Test post content"
        };

        _mockAuthorRepository
            .Setup(x => x.ExistsAsync(It.IsAny<AuthorId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("description");

        _mockPostRepository.Verify(
            x => x.AddAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidContent_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test Post Title",
            Description = "Test post description",
            Content = "" // Invalid empty content
        };

        _mockAuthorRepository
            .Setup(x => x.ExistsAsync(It.IsAny<AuthorId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("content");

        _mockPostRepository.Verify(
            x => x.AddAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentDateTime()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            AuthorId = Guid.NewGuid(),
            Title = "Test Post Title",
            Description = "Test post description",
            Content = "Test post content"
        };

        _mockAuthorRepository
            .Setup(x => x.ExistsAsync(It.IsAny<AuthorId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var expectedResponse = new CreatePostResponse
        {
            Id = Guid.NewGuid(),
            CreatedAt = _testDateTime
        };

        _mockMapper
            .Setup(x => x.Map<CreatePostResponse>(It.IsAny<Post>()))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CreatedAt.Should().Be(_testDateTime);
        _mockDateTime.Verify(x => x.UtcNow, Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreatePostWithCorrectAuthorId()
    {
        // Arrange
        var authorId = AuthorId.CreateUnique();
        var command = new CreatePostCommand
        {
            AuthorId = authorId.Value,
            Title = "Test Post Title",
            Description = "Test post description",
            Content = "Test post content"
        };

        Post? capturedPost = null;
        _mockPostRepository
            .Setup(x => x.AddAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()))
            .Callback<Post, CancellationToken>((post, ct) => capturedPost = post);

        _mockAuthorRepository
            .Setup(x => x.ExistsAsync(It.IsAny<AuthorId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var expectedResponse = new CreatePostResponse { Id = Guid.NewGuid() };
        _mockMapper
            .Setup(x => x.Map<CreatePostResponse>(It.IsAny<Post>()))
            .Returns(expectedResponse);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedPost.Should().NotBeNull();
        capturedPost!.AuthorId.Value.Should().Be(authorId.Value);
    }
}
