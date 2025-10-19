using System.Xml.Serialization;

namespace Yuki.Blog.Api.Contracts.v1.Responses;

/// <summary>
/// Response model for getting blog post information.
/// </summary>
[XmlRoot("GetPostResponse")]
public record GetPostResponse
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

    /// <summary>
    /// Author information (only populated if requested).
    /// </summary>
    [XmlElement("Author", IsNullable = true)]
    public AuthorInfo? Author { get; init; }

    /// <summary>
    /// Nested response model for author information within GetPostResponse.
    /// </summary>
    [XmlRoot("Author")]
    public record AuthorInfo
    {
        /// <summary>
        /// The unique identifier of the author.
        /// </summary>
        [XmlElement("Id")]
        public Guid Id { get; init; }

        /// <summary>
        /// The author's first name.
        /// </summary>
        [XmlElement("Name")]
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// The author's last name.
        /// </summary>
        [XmlElement("Surname")]
        public string Surname { get; init; } = string.Empty;
    }
}
