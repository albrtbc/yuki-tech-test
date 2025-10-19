using Bogus;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Infrastructure.Persistence;

namespace Yuki.Blog.Api.E2ETests.Builders;

/// <summary>
/// Builder pattern for creating Author test data with sensible defaults.
/// Uses Bogus for generating realistic fake data.
/// </summary>
public class AuthorBuilder
{
    private readonly Faker _faker = new Faker();
    private string _name;
    private string _surname;
    private DateTime _createdAt;

    public AuthorBuilder()
    {
        // Set sensible defaults using Bogus
        _name = _faker.Name.FirstName();
        _surname = _faker.Name.LastName();
        _createdAt = DateTime.UtcNow;
    }

    public AuthorBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public AuthorBuilder WithSurname(string surname)
    {
        _surname = surname;
        return this;
    }

    public AuthorBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Creates the author entity without saving to database.
    /// </summary>
    public Author Build()
    {
        var result = Author.Create(_name, _surname, _createdAt);
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create author: {result.ErrorMessage}");
        }
        return result.Value;
    }

    /// <summary>
    /// Creates and saves the author to the database, returning its ID.
    /// </summary>
    public async Task<Guid> BuildAndSaveAsync(BlogDbContext context)
    {
        var author = Build();
        context.Authors.Add(author);
        await context.SaveChangesAsync();
        return author.Id.Value;
    }

    /// <summary>
    /// Creates a default author with "Albert Blanco" for consistency with existing tests.
    /// </summary>
    public static AuthorBuilder DefaultAlbert() =>
        new AuthorBuilder()
            .WithName("Albert")
            .WithSurname("Blanco");
}
