using Microsoft.AspNetCore.Mvc;

namespace Yuki.Blog.Api.Results;

/// <summary>
/// An ObjectResult that returns a 403 Forbidden status code with ProblemDetails.
/// Provides consistency with other error result types in the API.
/// </summary>
public class ForbiddenObjectResult : ObjectResult
{
    /// <summary>
    /// Initializes a new instance of the ForbiddenObjectResult class with the specified value.
    /// </summary>
    /// <param name="value">The value to format in the entity body, typically ProblemDetails.</param>
    public ForbiddenObjectResult(object? value) : base(value)
    {
        StatusCode = StatusCodes.Status403Forbidden;
    }
}
