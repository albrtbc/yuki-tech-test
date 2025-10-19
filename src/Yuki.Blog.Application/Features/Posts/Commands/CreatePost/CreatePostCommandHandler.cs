using AutoMapper;
using MediatR;
using Yuki.Blog.Application.Common.Interfaces;
using Yuki.Blog.Application.Common.Models;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.Repositories;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Application.Features.Posts.Commands.CreatePost;

/// <summary>
/// Handles the CreatePostCommand.
/// Orchestrates the creation of a new post using domain entities and repositories.
/// </summary>
public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, ApplicationResult<CreatePostResponse>>
{
    private readonly IPostRepository _postRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTime _dateTime;

    public CreatePostCommandHandler(
        IPostRepository postRepository,
        IAuthorRepository authorRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDateTime dateTime)
    {
        _postRepository = postRepository;
        _authorRepository = authorRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
    }

    public async Task<ApplicationResult<CreatePostResponse>> Handle(
        CreatePostCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify author exists
        var authorIdResult = AuthorId.Create(request.AuthorId);
        if (authorIdResult.IsFailure)
        {
            return ApplicationResult<CreatePostResponse>.Failure(Error.Validation(authorIdResult.ErrorMessage));
        }

        var authorId = authorIdResult.Value;
        var authorExists = await _authorRepository.ExistsAsync(authorId, cancellationToken);

        if (!authorExists)
        {
            return ApplicationResult<CreatePostResponse>.Failure(Error.NotFound($"Author with ID '{request.AuthorId}' was not found."));
        }

        // 2. Create domain entity (validation happens in factory method)
        // Domain returns DomainResult, we convert to ApplicationResult with Error
        var postResult = Post.Create(
            authorId,
            request.Title,
            request.Description,
            request.Content,
            _dateTime.UtcNow);

        if (postResult.IsFailure)
        {
            // Convert domain error message to application Error
            return ApplicationResult<CreatePostResponse>.Failure(Error.Validation(postResult.ErrorMessage));
        }

        // 3. Persist to repository
        await _postRepository.AddAsync(postResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Map to DTO and return success
        var response = _mapper.Map<CreatePostResponse>(postResult.Value);
        return ApplicationResult<CreatePostResponse>.Success(response);
    }
}
