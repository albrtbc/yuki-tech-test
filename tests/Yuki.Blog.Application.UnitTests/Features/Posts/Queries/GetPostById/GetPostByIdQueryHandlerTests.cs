using FluentAssertions;
using Moq;
using Xunit;
using Yuki.Blog.Application.Features.Posts.Queries.GetPostById;
using Yuki.Blog.Domain.ReadOnlyRepositories;

namespace Yuki.Blog.Application.UnitTests.Features.Posts.Queries.GetPostById;

public class GetPostByIdQueryHandlerTests
{
    private readonly Mock<IPostReadOnlyRepository> _mockPostReadOnlyRepository;
    private readonly Mock<IAuthorReadOnlyRepository> _mockAuthorReadOnlyRepository;
    private readonly GetPostByIdQueryHandler _handler;

    public GetPostByIdQueryHandlerTests()
    {
        _mockPostReadOnlyRepository = new Mock<IPostReadOnlyRepository>();
        _mockAuthorReadOnlyRepository = new Mock<IAuthorReadOnlyRepository>();

        _handler = new GetPostByIdQueryHandler(
            _mockPostReadOnlyRepository.Object,
            _mockAuthorReadOnlyRepository.Object);
    }

    [Fact]
    public async Task Handle_WithExistingPost_ShouldReturnSuccessResult()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var postReadDto = new PostReadDto
        {
            Id = postId,
            AuthorId = authorId,
            Title = "Test Post",
            Description = "Test Description",
            Content = "Test Content",
            CreatedAt = createdAt,
            UpdatedAt = null
        };

        var query = new GetPostByIdQuery(postId);

        _mockPostReadOnlyRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(postReadDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(postId);
        result.Value.AuthorId.Should().Be(authorId);
        result.Value.Title.Should().Be("Test Post");
        result.Value.Description.Should().Be("Test Description");
        result.Value.Content.Should().Be("Test Content");
        result.Value.CreatedAt.Should().Be(createdAt);
        result.Value.UpdatedAt.Should().BeNull();
        result.Value.Author.Should().BeNull();

        _mockAuthorReadOnlyRepository.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentPost_ShouldReturnFailureResult()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var query = new GetPostByIdQuery(postId);

        _mockPostReadOnlyRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PostReadDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Post");
        result.Error.Message.Should().Contain("not found");
        result.Error.Message.Should().Contain(postId.ToString());
    }

    [Fact]
    public async Task Handle_WithIncludeAuthorTrue_ShouldFetchAuthorData()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var postReadDto = new PostReadDto
        {
            Id = postId,
            AuthorId = authorId,
            Title = "Test Post",
            Description = "Test Description",
            Content = "Test Content",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        var authorReadDto = new AuthorReadDto
        {
            Id = authorId,
            Name = "Albert",
            Surname = "Blanco"
        };

        var query = new GetPostByIdQuery(postId, new[] { "author" });

        _mockPostReadOnlyRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(postReadDto);

        _mockAuthorReadOnlyRepository
            .Setup(x => x.GetByIdAsync(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorReadDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Author.Should().NotBeNull();
        result.Value.Author!.Id.Should().Be(authorId);
        result.Value.Author.Name.Should().Be("Albert");
        result.Value.Author.Surname.Should().Be("Blanco");

        _mockAuthorReadOnlyRepository.Verify(
            x => x.GetByIdAsync(authorId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithIncludeAuthorFalse_ShouldNotFetchAuthorData()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var postReadDto = new PostReadDto
        {
            Id = postId,
            AuthorId = authorId,
            Title = "Test Post",
            Description = "Test Description",
            Content = "Test Content",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        var query = new GetPostByIdQuery(postId);

        _mockPostReadOnlyRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(postReadDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Author.Should().BeNull();

        _mockAuthorReadOnlyRepository.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithIncludeAuthorTrueButAuthorNotFound_ShouldReturnPostWithoutAuthor()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var postReadDto = new PostReadDto
        {
            Id = postId,
            AuthorId = authorId,
            Title = "Test Post",
            Description = "Test Description",
            Content = "Test Content",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        var query = new GetPostByIdQuery(postId, new[] { "author" });

        _mockPostReadOnlyRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(postReadDto);

        _mockAuthorReadOnlyRepository
            .Setup(x => x.GetByIdAsync(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthorReadDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Author.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithUpdatedPost_ShouldReturnUpdatedAtValue()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-5);
        var updatedAt = DateTime.UtcNow;

        var postReadDto = new PostReadDto
        {
            Id = postId,
            AuthorId = authorId,
            Title = "Updated Post",
            Description = "Updated Description",
            Content = "Updated Content",
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        var query = new GetPostByIdQuery(postId);

        _mockPostReadOnlyRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(postReadDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UpdatedAt.Should().Be(updatedAt);
        result.Value.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task Handle_ShouldMapAllPostProperties()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var postReadDto = new PostReadDto
        {
            Id = postId,
            AuthorId = authorId,
            Title = "Specific Title",
            Description = "Specific Description",
            Content = "Specific Content",
            CreatedAt = createdAt,
            UpdatedAt = null
        };

        var query = new GetPostByIdQuery(postId);

        _mockPostReadOnlyRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(postReadDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Id.Should().Be(postId);
        result.Value.AuthorId.Should().Be(authorId);
        result.Value.Title.Should().Be("Specific Title");
        result.Value.Description.Should().Be("Specific Description");
        result.Value.Content.Should().Be("Specific Content");
        result.Value.CreatedAt.Should().Be(createdAt);
        result.Value.UpdatedAt.Should().BeNull();
    }
}
