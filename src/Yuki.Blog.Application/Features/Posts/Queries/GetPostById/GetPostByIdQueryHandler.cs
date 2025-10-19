using MediatR;
using Yuki.Blog.Application.Common.Models;
using Yuki.Blog.Domain.ReadOnlyRepositories;

namespace Yuki.Blog.Application.Features.Posts.Queries.GetPostById;

/// <summary>
/// Handles the GetPostByIdQuery.
/// Uses read-only repository pattern to separate read operations from domain layer.
/// Following CQRS principles: queries use read-only DTOs, commands use domain entities.
/// Composes post and author data from separate repositories for better separation of concerns.
/// Maps PostReadDto to PostResponse for the application layer.
/// </summary>
public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, ApplicationResult<GetPostByIdResponse>>
{
    private readonly IPostReadOnlyRepository _postReadOnlyRepository;
    private readonly IAuthorReadOnlyRepository _authorReadOnlyRepository;

    public GetPostByIdQueryHandler(
        IPostReadOnlyRepository postReadOnlyRepository,
        IAuthorReadOnlyRepository authorReadOnlyRepository)
    {
        _postReadOnlyRepository = postReadOnlyRepository;
        _authorReadOnlyRepository = authorReadOnlyRepository;
    }

    public async Task<ApplicationResult<GetPostByIdResponse>> Handle(
        GetPostByIdQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Query post via read-only repository - returns PostReadDto from database projection
        var postReadDto = await _postReadOnlyRepository.GetByIdAsync(request.PostId, cancellationToken);

        // 2. Check if post exists
        if (postReadDto is null)
        {
            return ApplicationResult<GetPostByIdResponse>.Failure(
                Error.NotFound($"Post with ID '{request.PostId}' was not found."));
        }

        // 3. Fetch author if requested - composition at application layer
        AuthorReadDto? authorReadDto = null;
        if (request.IncludeAuthor)
        {
            authorReadDto = await _authorReadOnlyRepository.GetByIdAsync(postReadDto.AuthorId, cancellationToken);
        }

        // 4. Map PostReadDto to PostResponse (simple property mapping)
        var response = new GetPostByIdResponse
        {
            Id = postReadDto.Id,
            AuthorId = postReadDto.AuthorId,
            Title = postReadDto.Title,
            Description = postReadDto.Description,
            Content = postReadDto.Content,
            CreatedAt = postReadDto.CreatedAt,
            UpdatedAt = postReadDto.UpdatedAt,
            Author = authorReadDto is not null
                ? new GetPostByIdAuthorResponse
                {
                    Id = authorReadDto.Id,
                    Name = authorReadDto.Name,
                    Surname = authorReadDto.Surname
                }
                : null
        };

        // 5. Return success
        return ApplicationResult<GetPostByIdResponse>.Success(response);
    }
}
