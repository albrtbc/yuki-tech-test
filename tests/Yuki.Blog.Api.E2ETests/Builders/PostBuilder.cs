using Bogus;
using Yuki.Blog.Api.Contracts.v1.Requests;

namespace Yuki.Blog.Api.E2ETests.Builders;

/// <summary>
/// Builder pattern for creating Post request test data with sensible defaults.
/// Uses Bogus for generating realistic fake data.
/// </summary>
public class PostBuilder
{
    private readonly Faker _faker = new Faker();
    private Guid _authorId;
    private string _title;
    private string _description;
    private string _content;

    public PostBuilder()
    {
        // Set sensible defaults using Bogus
        _authorId = Guid.Empty; // Must be set explicitly
        _title = _faker.Lorem.Sentence(5).TrimEnd('.');
        _description = _faker.Lorem.Sentence(10);
        _content = _faker.Lorem.Paragraphs(3);
    }

    public PostBuilder WithAuthorId(Guid authorId)
    {
        _authorId = authorId;
        return this;
    }

    public PostBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public PostBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public PostBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    /// <summary>
    /// Sets the title to a specific length (for boundary testing).
    /// </summary>
    public PostBuilder WithTitleOfLength(int length)
    {
        _title = new string('a', length);
        return this;
    }

    /// <summary>
    /// Sets the description to a specific length (for boundary testing).
    /// </summary>
    public PostBuilder WithDescriptionOfLength(int length)
    {
        _description = new string('a', length);
        return this;
    }

    /// <summary>
    /// Sets the content to a specific length (for boundary testing).
    /// </summary>
    public PostBuilder WithContentOfLength(int length)
    {
        _content = new string('a', length);
        return this;
    }

    /// <summary>
    /// Sets empty title for invalid scenario testing.
    /// </summary>
    public PostBuilder WithEmptyTitle()
    {
        _title = string.Empty;
        return this;
    }

    /// <summary>
    /// Sets empty description for invalid scenario testing.
    /// </summary>
    public PostBuilder WithEmptyDescription()
    {
        _description = string.Empty;
        return this;
    }

    /// <summary>
    /// Sets empty content for invalid scenario testing.
    /// </summary>
    public PostBuilder WithEmptyContent()
    {
        _content = string.Empty;
        return this;
    }

    /// <summary>
    /// Builds the CreatePostRequest.
    /// </summary>
    public CreatePostRequest Build()
    {
        if (_authorId == Guid.Empty)
        {
            throw new InvalidOperationException("AuthorId must be set before building a post request.");
        }

        return new CreatePostRequest
        {
            AuthorId = _authorId,
            Title = _title,
            Description = _description,
            Content = _content
        };
    }

    /// <summary>
    /// Creates a valid post request with sensible defaults.
    /// </summary>
    public static PostBuilder Valid(Guid authorId) =>
        new PostBuilder().WithAuthorId(authorId);
}
