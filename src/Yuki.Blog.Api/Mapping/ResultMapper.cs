using Microsoft.AspNetCore.Mvc;
using Yuki.Blog.Api.Results;
using Yuki.Blog.Application.Common.Models;

namespace Yuki.Blog.Api.Mapping;

/// <summary>
/// Default implementation of IResultMapper.
/// Converts Result objects to appropriate HTTP responses based on error types.
/// </summary>
public class ResultMapper : IResultMapper
{
    /// <inheritdoc />
    public IActionResult ToActionResult<T>(ApplicationResult<T> result, Func<T, IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess?.Invoke(result.Value) ?? new OkObjectResult(result.Value);
        }

        return MapErrorToActionResult(result.Error);
    }

    private static IActionResult MapErrorToActionResult(Error error)
    {
        var statusCode = GetStatusCodeForError(error);
        var title = GetTitleForError(error);

        var problemDetails = new ProblemDetails
        {
            Title = title,
            Status = statusCode,
            Detail = error.Message,
            Type = GetProblemTypeUri(statusCode)
        };

        return statusCode switch
        {
            StatusCodes.Status404NotFound => new NotFoundObjectResult(problemDetails),
            StatusCodes.Status400BadRequest => new BadRequestObjectResult(problemDetails),
            StatusCodes.Status409Conflict => new ConflictObjectResult(problemDetails),
            StatusCodes.Status401Unauthorized => new UnauthorizedObjectResult(problemDetails),
            StatusCodes.Status403Forbidden => new ForbiddenObjectResult(problemDetails),
            _ => new InternalServerErrorObjectResult(problemDetails)
        };
    }

    private static int GetStatusCodeForError(Error error)
    {
        return error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Internal => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string GetTitleForError(Error error)
    {
        return error.Type switch
        {
            ErrorType.NotFound => "Not Found",
            ErrorType.Validation => "Validation Error",
            ErrorType.Conflict => "Conflict",
            ErrorType.Unauthorized => "Unauthorized",
            ErrorType.Forbidden => "Forbidden",
            ErrorType.Internal => "Internal Server Error",
            _ => "Error"
        };
    }

    private static string GetProblemTypeUri(int statusCode)
    {
        return $"https://httpstatuses.com/{statusCode}";
    }
}
