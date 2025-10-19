using Yuki.Blog.Api.Contracts.v1.Requests;
using Yuki.Blog.Api.E2ETests.Builders;

namespace Yuki.Blog.Api.E2ETests.Helpers;

/// <summary>
/// Static helper methods for common test operations.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates a valid CreatePostRequest with sensible defaults.
    /// </summary>
    public static CreatePostRequest CreateValidPostRequest(Guid authorId) =>
        PostBuilder.Valid(authorId).Build();

    /// <summary>
    /// Creates a CreatePostRequest with an empty title (invalid).
    /// </summary>
    public static CreatePostRequest CreatePostRequestWithEmptyTitle(Guid authorId) =>
        PostBuilder.Valid(authorId)
            .WithEmptyTitle()
            .Build();

    /// <summary>
    /// Creates a CreatePostRequest with an empty description (invalid).
    /// </summary>
    public static CreatePostRequest CreatePostRequestWithEmptyDescription(Guid authorId) =>
        PostBuilder.Valid(authorId)
            .WithEmptyDescription()
            .Build();

    /// <summary>
    /// Creates a CreatePostRequest with an empty content (invalid).
    /// </summary>
    public static CreatePostRequest CreatePostRequestWithEmptyContent(Guid authorId) =>
        PostBuilder.Valid(authorId)
            .WithEmptyContent()
            .Build();

    /// <summary>
    /// Creates a CreatePostRequest with a title of specified length.
    /// </summary>
    public static CreatePostRequest CreatePostRequestWithTitleLength(Guid authorId, int length) =>
        PostBuilder.Valid(authorId)
            .WithTitleOfLength(length)
            .Build();

    /// <summary>
    /// Creates a CreatePostRequest with a description of specified length.
    /// </summary>
    public static CreatePostRequest CreatePostRequestWithDescriptionLength(Guid authorId, int length) =>
        PostBuilder.Valid(authorId)
            .WithDescriptionOfLength(length)
            .Build();

    /// <summary>
    /// Creates a CreatePostRequest with content of specified length.
    /// </summary>
    public static CreatePostRequest CreatePostRequestWithContentLength(Guid authorId, int length) =>
        PostBuilder.Valid(authorId)
            .WithContentOfLength(length)
            .Build();

    /// <summary>
    /// Creates a CreatePostRequest with special characters for encoding tests.
    /// </summary>
    public static CreatePostRequest CreatePostRequestWithSpecialCharacters(Guid authorId) =>
        new PostBuilder()
            .WithAuthorId(authorId)
            .WithTitle("Test with special chars: émojis 🚀 & symbols <>&\"'")
            .WithDescription("Description with UTF-8: café, résumé, naïve, Zürich")
            .WithContent("Content with newlines\nand\ttabs\rand special symbols: €£¥©®™")
            .Build();

    /// <summary>
    /// Creates XML content for a CreatePostRequest.
    /// </summary>
    public static string CreateXmlPostRequest(Guid authorId, string title) =>
        $@"<?xml version=""1.0"" encoding=""utf-8""?>
<CreatePostRequest>
    <AuthorId>{authorId}</AuthorId>
    <Title>{title}</Title>
    <Description>Post created via XML</Description>
    <Content>This post was submitted as XML content</Content>
</CreatePostRequest>";
}
