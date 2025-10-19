using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Yuki.Blog.Api.Contracts.v1.Requests;

/// <summary>
/// Query parameters for post retrieval operations.
/// </summary>
public class PostQueryParameters
{
    /// <summary>
    /// Comma-separated list of related resources to include.
    /// Supported values: "author", "tags", "comments".
    /// Example: ?include=author,tags
    /// </summary>
    [FromQuery(Name = "include")]
    public string? IncludeRaw { get; init; }

    /// <summary>
    /// Gets the parsed list of includes.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<string> Includes =>
        string.IsNullOrWhiteSpace(IncludeRaw)
            ? Array.Empty<string>()
            : IncludeRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
