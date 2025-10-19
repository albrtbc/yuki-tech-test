using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.ValueObjects;
using Yuki.Blog.Infrastructure.Persistence;
using Yuki.Blog.Infrastructure.Persistence.ReadOnlyRepositories;

namespace Yuki.Blog.Infrastructure.UnitTests.Persistence.ReadOnlyRepositories;

public class AuthorReadOnlyRepositoryTests
{
    private readonly Mock<IPublisher> _publisherMock;
    private readonly Mock<ILogger<BlogDbContext>> _loggerMock;

    public AuthorReadOnlyRepositoryTests()
    {
        _publisherMock = new Mock<IPublisher>();
        _loggerMock = new Mock<ILogger<BlogDbContext>>();
    }

    private BlogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new BlogDbContext(options, _publisherMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAuthorExists_ShouldReturnAuthorReadDto()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorReadOnlyRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(authorId.Value);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(authorId.Value);
        result.Name.Should().Be("Albert");
        result.Surname.Should().Be("Blanco");
    }

    [Fact]
    public async Task GetByIdAsync_WhenAuthorDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorReadOnlyRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldUseAsNoTracking()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorReadOnlyRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Clear the change tracker to simulate a fresh query
        context.ChangeTracker.Clear();

        // Act
        var result = await repository.GetByIdAsync(authorId.Value);

        // Assert
        result.Should().NotBeNull();

        // Verify no entities are tracked (AsNoTracking ensures this)
        context.ChangeTracker.Entries<Author>().Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldProjectDirectlyToDto()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorReadOnlyRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Jane", "Smith", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(authorId.Value);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(authorId.Value);
        result.Name.Should().Be("Jane");
        result.Surname.Should().Be("Smith");
    }

    [Fact]
    public async Task GetByIdAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorReadOnlyRepository(context);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await repository.GetByIdAsync(Guid.NewGuid(), cts.Token));
    }

    [Fact]
    public async Task GetByIdAsync_WithMultipleAuthors_ShouldReturnCorrectAuthor()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorReadOnlyRepository(context);

        // Add multiple authors
        var author1Id = AuthorId.Create(Guid.NewGuid()).Value;
        var author2Id = AuthorId.Create(Guid.NewGuid()).Value;
        var author3Id = AuthorId.Create(Guid.NewGuid()).Value;

        var author1 = TestHelpers.CreateAuthor(author1Id, "Albert", "Blanco", DateTime.UtcNow);
        var author2 = TestHelpers.CreateAuthor(author2Id, "Jane", "Smith", DateTime.UtcNow);
        var author3 = TestHelpers.CreateAuthor(author3Id, "Bob", "Johnson", DateTime.UtcNow);

        context.Authors.AddRange(author1, author2, author3);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(author2Id.Value);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(author2Id.Value);
        result.Name.Should().Be("Jane");
        result.Surname.Should().Be("Smith");
    }

    [Fact]
    public async Task GetByIdAsync_WithSpecialCharactersInName_ShouldReturnCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorReadOnlyRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "José", "O'Brien", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(authorId.Value);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(authorId.Value);
        result.Name.Should().Be("José");
        result.Surname.Should().Be("O'Brien");
    }
}
