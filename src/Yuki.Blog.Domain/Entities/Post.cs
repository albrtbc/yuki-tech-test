using Yuki.Blog.Domain.Common;
using Yuki.Blog.Domain.Events;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Domain.Entities;

/// <summary>
/// Post aggregate root representing a blog post in the system.
/// Encapsulates all business logic related to posts.
/// </summary>
public sealed class Post : AggregateRoot<PostId>
{
    /// <summary>
    /// The ID of the author who created this post.
    /// </summary>
    public AuthorId AuthorId { get; private set; }

    /// <summary>
    /// The title of the post.
    /// </summary>
    public PostTitle Title { get; private set; }

    /// <summary>
    /// A brief description or summary of the post.
    /// </summary>
    public PostDescription Description { get; private set; }

    /// <summary>
    /// The full content of the post.
    /// </summary>
    public PostContent Content { get; private set; }

    /// <summary>
    /// The date and time when the post was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// The date and time when the post was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Post() : base()
    {
        Title = null!;
        Description = null!;
        Content = null!;
        AuthorId = null!;
    }

    private Post(PostId id, AuthorId authorId, PostTitle title, PostDescription description, PostContent content, DateTime createdAt)
        : base(id)
    {
        AuthorId = authorId;
        Title = title;
        Description = description;
        Content = content;
        CreatedAt = createdAt;
        UpdatedAt = null;
    }

    /// <summary>
    /// Validates the post data and returns the validated value objects.
    /// </summary>
    /// <param name="authorId">The ID of the post's author.</param>
    /// <param name="title">The post title.</param>
    /// <param name="description">The post description.</param>
    /// <param name="content">The post content.</param>
    /// <returns>A Result containing the validated value objects if valid, or an error message if invalid.</returns>
    private static DomainResult<(PostTitle Title, PostDescription Description, PostContent Content)> ValidatePostData(
        AuthorId authorId, string title, string description, string content)
    {
        // Validate parameters
        if (authorId == null)
        {
            return DomainResult<(PostTitle, PostDescription, PostContent)>.Failure("Author ID cannot be null.");
        }

        var postTitleResult = PostTitle.Create(title);
        if (postTitleResult.IsFailure)
        {
            return DomainResult<(PostTitle, PostDescription, PostContent)>.Failure(postTitleResult.ErrorMessage);
        }

        var postDescriptionResult = PostDescription.Create(description);
        if (postDescriptionResult.IsFailure)
        {
            return DomainResult<(PostTitle, PostDescription, PostContent)>.Failure(postDescriptionResult.ErrorMessage);
        }

        var postContentResult = PostContent.Create(content);
        if (postContentResult.IsFailure)
        {
            return DomainResult<(PostTitle, PostDescription, PostContent)>.Failure(postContentResult.ErrorMessage);
        }

        return DomainResult<(PostTitle, PostDescription, PostContent)>.Success(
            (postTitleResult.Value, postDescriptionResult.Value, postContentResult.Value));
    }

    /// <summary>
    /// Factory method to create a new Post.
    /// </summary>
    /// <param name="authorId">The ID of the post's author.</param>
    /// <param name="title">The post title.</param>
    /// <param name="description">The post description.</param>
    /// <param name="content">The post content.</param>
    /// <param name="createdAt">The date and time when the post is created.</param>
    /// <returns>A Result containing the new Post if valid, or an error message if invalid.</returns>
    public static DomainResult<Post> Create(AuthorId authorId, string title, string description, string content, DateTime createdAt)
    {
        var validationResult = ValidatePostData(authorId, title, description, content);
        if (validationResult.IsFailure)
        {
            return DomainResult<Post>.Failure(validationResult.ErrorMessage);
        }

        var (postTitle, postDescription, postContent) = validationResult.Value;
        var postId = PostId.CreateUnique();
        var post = new Post(postId, authorId, postTitle, postDescription, postContent, createdAt);

        // Raise domain event
        post.RaiseDomainEvent(new PostCreatedEvent(postId, authorId, postTitle.Value, createdAt));

        return DomainResult<Post>.Success(post);
    }

    /// <summary>
    /// Factory method to create a new Post with a specific ID (primarily for testing purposes).
    /// </summary>
    /// <param name="id">The specific ID for the post.</param>
    /// <param name="authorId">The ID of the post's author.</param>
    /// <param name="title">The post title.</param>
    /// <param name="description">The post description.</param>
    /// <param name="content">The post content.</param>
    /// <param name="createdAt">The date and time when the post is created.</param>
    /// <returns>A Result containing the new Post if valid, or an error message if invalid.</returns>
    public static DomainResult<Post> CreateWithId(Guid id, AuthorId authorId, string title, string description, string content, DateTime createdAt)
    {
        var validationResult = ValidatePostData(authorId, title, description, content);
        if (validationResult.IsFailure)
        {
            return DomainResult<Post>.Failure(validationResult.ErrorMessage);
        }

        var (postTitle, postDescription, postContent) = validationResult.Value;
        var postIdResult = PostId.Create(id);
        if (postIdResult.IsFailure)
        {
            return DomainResult<Post>.Failure(postIdResult.ErrorMessage);
        }

        var postId = postIdResult.Value;
        var post = new Post(postId, authorId, postTitle, postDescription, postContent, createdAt);

        // Raise domain event
        post.RaiseDomainEvent(new PostCreatedEvent(postId, authorId, postTitle.Value, createdAt));

        return DomainResult<Post>.Success(post);
    }

    /// <summary>
    /// Updates the post's content.
    /// </summary>
    /// <param name="newContent">The new content.</param>
    /// <param name="updatedAt">The date and time when the post is updated.</param>
    /// <returns>A Result indicating success or failure with an error message.</returns>
    public DomainResult UpdateContent(string newContent, DateTime updatedAt)
    {
        var postContentResult = PostContent.Create(newContent);
        if (postContentResult.IsFailure)
        {
            return DomainResult.Failure(postContentResult.ErrorMessage);
        }

        Content = postContentResult.Value;
        UpdatedAt = updatedAt;
        return DomainResult.Success();
    }

    /// <summary>
    /// Updates the post's title and description.
    /// </summary>
    /// <param name="newTitle">The new title.</param>
    /// <param name="newDescription">The new description.</param>
    /// <param name="updatedAt">The date and time when the post is updated.</param>
    /// <returns>A Result indicating success or failure with an error message.</returns>
    public DomainResult UpdateTitleAndDescription(string newTitle, string newDescription, DateTime updatedAt)
    {
        var postTitleResult = PostTitle.Create(newTitle);
        if (postTitleResult.IsFailure)
        {
            return DomainResult.Failure(postTitleResult.ErrorMessage);
        }

        var postDescriptionResult = PostDescription.Create(newDescription);
        if (postDescriptionResult.IsFailure)
        {
            return DomainResult.Failure(postDescriptionResult.ErrorMessage);
        }

        Title = postTitleResult.Value;
        Description = postDescriptionResult.Value;
        UpdatedAt = updatedAt;
        return DomainResult.Success();
    }
}
