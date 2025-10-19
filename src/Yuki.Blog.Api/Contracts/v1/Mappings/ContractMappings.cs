using Yuki.Blog.Api.Contracts.v1.Requests;
using Yuki.Blog.Api.Contracts.v1.Responses;
using Yuki.Blog.Application.Features.Posts.Queries.GetPostById;
using AppCreatePostResponse = Yuki.Blog.Application.Features.Posts.Commands.CreatePost.CreatePostResponse;
using AppCreatePostCommand = Yuki.Blog.Application.Features.Posts.Commands.CreatePost.CreatePostCommand;

namespace Yuki.Blog.Api.Contracts.v1.Mappings;

/// <summary>
/// Extension methods for mapping between API contracts and Application models.
/// </summary>
public static class ContractMappings
{
    /// <summary>
    /// Maps a CreatePostRequest DTO to a CreatePostCommand.
    /// </summary>
    public static AppCreatePostCommand ToCommand(this CreatePostRequest request)
    {
        return new AppCreatePostCommand
        {
            AuthorId = request.AuthorId,
            Title = request.Title,
            Description = request.Description,
            Content = request.Content
        };
    }

    /// <summary>
    /// Maps an Application CreatePostResponse to an API CreatePostResponse.
    /// </summary>
    public static CreatePostResponse ToApiResponse(this AppCreatePostResponse appResponse)
    {
        return new CreatePostResponse
        {
            Id = appResponse.Id,
            AuthorId = appResponse.AuthorId,
            Title = appResponse.Title,
            Description = appResponse.Description,
            Content = appResponse.Content,
            CreatedAt = appResponse.CreatedAt,
            UpdatedAt = appResponse.UpdatedAt
        };
    }

    /// <summary>
    /// Maps an Application GetPostByIdResponse to an API GetPostResponse.
    /// </summary>
    public static GetPostResponse ToApiResponse(this GetPostByIdResponse appResponse)
    {
        return new GetPostResponse
        {
            Id = appResponse.Id,
            AuthorId = appResponse.AuthorId,
            Title = appResponse.Title,
            Description = appResponse.Description,
            Content = appResponse.Content,
            CreatedAt = appResponse.CreatedAt,
            UpdatedAt = appResponse.UpdatedAt,
            Author = appResponse.Author?.ToAuthorInfo()
        };
    }

    /// <summary>
    /// Maps an Application GetPostByIdAuthorResponse to an API GetPostResponse.AuthorInfo.
    /// </summary>
    public static GetPostResponse.AuthorInfo ToAuthorInfo(this GetPostByIdAuthorResponse appResponse)
    {
        return new GetPostResponse.AuthorInfo
        {
            Id = appResponse.Id,
            Name = appResponse.Name,
            Surname = appResponse.Surname
        };
    }
}
