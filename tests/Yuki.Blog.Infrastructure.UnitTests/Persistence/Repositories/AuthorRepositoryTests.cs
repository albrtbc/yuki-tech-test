using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.ValueObjects;
using Yuki.Blog.Infrastructure.Persistence;
using Yuki.Blog.Infrastructure.Persistence.Repositories;

namespace Yuki.Blog.Infrastructure.UnitTests.Persistence.Repositories;

public class AuthorRepositoryTests
{
    private readonly Mock<IPublisher> _publisherMock;
    private readonly Mock<ILogger<BlogDbContext>> _loggerMock;

    public AuthorRepositoryTests()
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
    public async Task GetByIdAsync_WhenAuthorExists_ShouldReturnAuthor()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(authorId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(authorId);
        result.Name.Should().Be("Albert");
        result.Surname.Should().Be("Blanco");
    }

    [Fact]
    public async Task GetByIdAsync_WhenAuthorDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorRepository(context);
        var nonExistentId = AuthorId.Create(Guid.NewGuid()).Value;

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddAuthorToContext()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Jane", "Smith", DateTime.UtcNow);

        // Act
        await repository.AddAsync(author);
        await context.SaveChangesAsync();

        // Assert
        var savedAuthor = await context.Authors.FindAsync(authorId);
        savedAuthor.Should().NotBeNull();
        savedAuthor!.Id.Should().Be(authorId);
        savedAuthor.Name.Should().Be("Jane");
        savedAuthor.Surname.Should().Be("Smith");
    }

    [Fact]
    public async Task Update_ShouldMarkAuthorAsModified()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Detach to simulate loading from another context
        context.Entry(author).State = EntityState.Detached;

        var updatedAuthor = TestHelpers.CreateAuthor(authorId, "Albert", "Smith", DateTime.UtcNow);

        // Act
        repository.Update(updatedAuthor);
        await context.SaveChangesAsync();

        // Assert
        var savedAuthor = await context.Authors.FindAsync(authorId);
        savedAuthor.Should().NotBeNull();
        savedAuthor!.Surname.Should().Be("Smith");
    }

    [Fact]
    public async Task Remove_ShouldDeleteAuthorFromContext()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Act
        repository.Remove(author);
        await context.SaveChangesAsync();

        // Assert
        var deletedAuthor = await context.Authors.FindAsync(authorId);
        deletedAuthor.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WhenAuthorExists_ShouldReturnTrue()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.ExistsAsync(authorId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenAuthorDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorRepository(context);
        var nonExistentId = AuthorId.Create(Guid.NewGuid()).Value;

        // Act
        var result = await repository.ExistsAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorRepository(context);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await repository.GetByIdAsync(AuthorId.Create(Guid.NewGuid()).Value, cts.Token));
    }

    [Fact]
    public async Task ExistsAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorRepository(context);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await repository.ExistsAsync(AuthorId.Create(Guid.NewGuid()).Value, cts.Token));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldEnableChangeTracking()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorRepository(context);

        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Clear change tracker
        context.ChangeTracker.Clear();

        // Act
        var result = await repository.GetByIdAsync(authorId);

        // Assert
        result.Should().NotBeNull();
        context.ChangeTracker.Entries<Author>().Should().NotBeEmpty();
        context.Entry(result!).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task AddAsync_WithMultipleAuthors_ShouldAddAll()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AuthorRepository(context);

        var author1 = TestHelpers.CreateAuthor(AuthorId.Create(Guid.NewGuid()).Value, "Albert", "Blanco", DateTime.UtcNow);
        var author2 = TestHelpers.CreateAuthor(AuthorId.Create(Guid.NewGuid()).Value, "Jane", "Smith", DateTime.UtcNow);
        var author3 = TestHelpers.CreateAuthor(AuthorId.Create(Guid.NewGuid()).Value, "Bob", "Johnson", DateTime.UtcNow);

        // Act
        await repository.AddAsync(author1);
        await repository.AddAsync(author2);
        await repository.AddAsync(author3);
        await context.SaveChangesAsync();

        // Assert
        var authors = await context.Authors.ToListAsync();
        authors.Should().HaveCount(3);
    }
}
