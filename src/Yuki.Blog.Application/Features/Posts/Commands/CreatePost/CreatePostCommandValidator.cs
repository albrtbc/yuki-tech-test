using FluentValidation;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Application.Features.Posts.Commands.CreatePost;

/// <summary>
/// Validator for CreatePostCommand.
/// Validates input before the command handler executes.
/// </summary>
public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.AuthorId)
            .NotEmpty()
            .WithMessage("Author ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(PostTitle.MaxLength)
            .WithMessage($"Title cannot exceed {PostTitle.MaxLength} characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(PostDescription.MaxLength)
            .WithMessage($"Description cannot exceed {PostDescription.MaxLength} characters.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required.")
            .MaximumLength(PostContent.MaxLength)
            .WithMessage($"Content cannot exceed {PostContent.MaxLength} characters.");
    }
}
