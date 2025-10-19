using System.Xml.Serialization;

namespace Yuki.Blog.Api.Contracts.v1.Requests;

/// <summary>
/// Request model for creating a new blog post.
/// Validation rules are defined in CreatePostCommandValidator using FluentValidation.
/// </summary>
[XmlRoot("CreatePostRequest")]
public record CreatePostRequest
{
    /// <summary>
    /// The ID of the author creating the post.
    /// </summary>
    [XmlElement("AuthorId")]
    public Guid AuthorId { get; init; }

    /// <summary>
    /// The title of the post. Maximum 200 characters.
    /// </summary>
    [XmlElement("Title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// A brief description of the post. Maximum 500 characters.
    /// </summary>
    [XmlElement("Description")]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The full content of the post. Maximum 50,000 characters.
    /// </summary>
    [XmlElement("Content")]
    public string Content { get; init; } = string.Empty;
}
