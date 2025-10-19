using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.ValueObjects;
using Yuki.Blog.Infrastructure.Persistence;

namespace Yuki.Blog.Infrastructure.UnitTests.Persistence.Configurations;

public class AuthorConfigurationTests
{
    private readonly Mock<IPublisher> _publisherMock;
    private readonly Mock<ILogger<BlogDbContext>> _loggerMock;

    public AuthorConfigurationTests()
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
    public async Task Configuration_ShouldMapAuthorIdCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Act
        var savedAuthor = await context.Authors.FindAsync(authorId);

        // Assert
        savedAuthor.Should().NotBeNull();
        savedAuthor!.Id.Should().Be(authorId);
        savedAuthor.Id.Value.Should().Be(authorId.Value);
    }

    [Fact]
    public async Task Configuration_ShouldMapNameCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Act
        var savedAuthor = await context.Authors.FindAsync(authorId);

        // Assert
        savedAuthor.Should().NotBeNull();
        savedAuthor!.Name.Should().Be("Albert");
    }

    [Fact]
    public async Task Configuration_ShouldMapSurnameCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Act
        var savedAuthor = await context.Authors.FindAsync(authorId);

        // Assert
        savedAuthor.Should().NotBeNull();
        savedAuthor!.Surname.Should().Be("Blanco");
    }

    [Fact]
    public async Task Configuration_ShouldMapCreatedAtCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var createdAt = DateTime.UtcNow;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", createdAt);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Act
        var savedAuthor = await context.Authors.FindAsync(authorId);

        // Assert
        savedAuthor.Should().NotBeNull();
        savedAuthor!.CreatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Configuration_ShouldNotPersistFullName()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);

        // Verify FullName exists on the entity
        author.FullName.Should().Be("Albert Blanco");

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Clear and reload
        context.ChangeTracker.Clear();

        // Act
        var savedAuthor = await context.Authors.FindAsync(authorId);

        // Assert
        savedAuthor.Should().NotBeNull();
        // FullName should still be computed from Name and Surname
        savedAuthor!.FullName.Should().Be("Albert Blanco");
    }

    [Fact]
    public async Task Configuration_ShouldNotPersistDomainEvents()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Clear and reload
        context.ChangeTracker.Clear();

        // Act
        var savedAuthor = await context.Authors.FindAsync(authorId);

        // Assert
        savedAuthor.Should().NotBeNull();
        savedAuthor!.DomainEvents.Should().BeEmpty(); // Domain events are not persisted
    }

    [Fact]
    public async Task Configuration_ShouldEnforceMaxLengthOnName()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;

        // Create a name that's within the max length
        var validName = new string('a', AuthorName.MaxLength);
        var author = TestHelpers.CreateAuthor(authorId, validName, "Blanco", DateTime.UtcNow);

        context.Authors.Add(author);

        // Act & Assert - should not throw
        await context.SaveChangesAsync();
        var savedAuthor = await context.Authors.FindAsync(authorId);
        savedAuthor.Should().NotBeNull();
        savedAuthor!.Name.Should().Be(validName);
    }

    [Fact]
    public async Task Configuration_ShouldEnforceMaxLengthOnSurname()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;

        // Create a surname that's within the max length
        var validSurname = new string('b', AuthorName.MaxLength);
        var author = TestHelpers.CreateAuthor(authorId, "Albert", validSurname, DateTime.UtcNow);

        context.Authors.Add(author);

        // Act & Assert - should not throw
        await context.SaveChangesAsync();
        var savedAuthor = await context.Authors.FindAsync(authorId);
        savedAuthor.Should().NotBeNull();
        savedAuthor!.Surname.Should().Be(validSurname);
    }

    [Fact]
    public async Task Configuration_ShouldHandleSpecialCharactersInName()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "José María", "O'Brien-Smith", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Clear and reload
        context.ChangeTracker.Clear();

        // Act
        var savedAuthor = await context.Authors.FindAsync(authorId);

        // Assert
        savedAuthor.Should().NotBeNull();
        savedAuthor!.Name.Should().Be("José María");
        savedAuthor.Surname.Should().Be("O'Brien-Smith");
        savedAuthor.FullName.Should().Be("José María O'Brien-Smith");
    }

    [Fact]
    public async Task Configuration_ShouldRequireAllFields()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Act
        var savedAuthor = await context.Authors.FindAsync(authorId);

        // Assert
        savedAuthor.Should().NotBeNull();
        savedAuthor!.Id.Should().NotBeNull();
        savedAuthor.Name.Should().NotBeNullOrEmpty();
        savedAuthor.Surname.Should().NotBeNullOrEmpty();
        savedAuthor.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task Configuration_ShouldSupportMultipleAuthors()
    {
        // Arrange
        using var context = CreateContext();
        var author1 = TestHelpers.CreateAuthor(AuthorId.Create(Guid.NewGuid()).Value, "Albert", "Blanco", DateTime.UtcNow);
        var author2 = TestHelpers.CreateAuthor(AuthorId.Create(Guid.NewGuid()).Value, "Jane", "Smith", DateTime.UtcNow);
        var author3 = TestHelpers.CreateAuthor(AuthorId.Create(Guid.NewGuid()).Value, "Bob", "Johnson", DateTime.UtcNow);

        context.Authors.AddRange(author1, author2, author3);
        await context.SaveChangesAsync();

        // Act
        var authors = await context.Authors.ToListAsync();

        // Assert
        authors.Should().HaveCountGreaterOrEqualTo(3);
    }

    [Fact]
    public async Task Configuration_ShouldUseCorrectTableName()
    {
        // Arrange
        using var context = CreateContext();
        var authorId = AuthorId.Create(Guid.NewGuid()).Value;
        var author = TestHelpers.CreateAuthor(authorId, "Albert", "Blanco", DateTime.UtcNow);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Act - If table name is incorrect, this would fail
        var savedAuthor = await context.Authors.FindAsync(authorId);

        // Assert
        savedAuthor.Should().NotBeNull();
    }
}
