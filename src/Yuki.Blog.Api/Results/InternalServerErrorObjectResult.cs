using Microsoft.AspNetCore.Mvc;

namespace Yuki.Blog.Api.Results;

/// <summary>
/// An ObjectResult that returns a 500 Internal Server Error status code with ProblemDetails.
/// Provides consistency with other error result types in the API.
/// </summary>
public class InternalServerErrorObjectResult : ObjectResult
{
    /// <summary>
    /// Initializes a new instance of the InternalServerErrorObjectResult class with the specified value.
    /// </summary>
    /// <param name="value">The value to format in the entity body, typically ProblemDetails.</param>
    public InternalServerErrorObjectResult(object? value) : base(value)
    {
        StatusCode = StatusCodes.Status500InternalServerError;
    }
}
