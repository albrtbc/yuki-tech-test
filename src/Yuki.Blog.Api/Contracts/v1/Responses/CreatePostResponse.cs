using System.Xml.Serialization;

namespace Yuki.Blog.Api.Contracts.v1.Responses;

/// <summary>
/// Response model for post creation.
/// </summary>
[XmlRoot("CreatePostResponse")]
public record CreatePostResponse
{
    /// <summary>
    /// The unique identifier of the post.
    /// </summary>
    [XmlElement("Id")]
    public Guid Id { get; init; }

    /// <summary>
    /// The ID of the author who created the post.
    /// </summary>
    [XmlElement("AuthorId")]
    public Guid AuthorId { get; init; }

    /// <summary>
    /// The title of the post.
    /// </summary>
    [XmlElement("Title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// A brief description of the post.
    /// </summary>
    [XmlElement("Description")]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The full content of the post.
    /// </summary>
    [XmlElement("Content")]
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// When the post was created.
    /// </summary>
    [XmlElement("CreatedAt")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the post was last updated (null if never updated).
    /// </summary>
    [XmlElement("UpdatedAt", IsNullable = true)]
    public DateTime? UpdatedAt { get; init; }
}
