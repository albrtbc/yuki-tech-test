using Microsoft.AspNetCore.Mvc;
using Yuki.Blog.Application.Common.Models;

namespace Yuki.Blog.Api.Mapping;

/// <summary>
/// Maps Result objects to IActionResult responses.
/// </summary>
public interface IResultMapper
{
    /// <summary>
    /// Converts a Result to an IActionResult, using the provided success handler if the result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="onSuccess">Optional function to handle successful results. If not provided, returns Ok(result.Value).</param>
    /// <returns>An IActionResult representing the result.</returns>
    IActionResult ToActionResult<T>(ApplicationResult<T> result, Func<T, IActionResult>? onSuccess = null);
}
