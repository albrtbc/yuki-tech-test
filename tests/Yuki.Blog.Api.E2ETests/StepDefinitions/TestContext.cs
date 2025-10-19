using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Yuki.Blog.Api.Contracts.v1.Requests;
using Yuki.Blog.Api.Contracts.v1.Responses;
using Yuki.Blog.Api.E2ETests.Builders;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Infrastructure.Persistence;

namespace Yuki.Blog.Api.E2ETests.StepDefinitions;

/// <summary>
/// Shared context for SpecFlow scenarios to maintain state between steps.
/// </summary>
public class TestContext
{
    private readonly BlogApiFactory _factory;
    private readonly HttpClient _client;

    public TestContext(BlogApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-API-Version", "1.0");
    }

    // Properties to store state between steps
    public Guid? AuthorId { get; set; }
    public Guid? PostId { get; set; }
    public CreatePostRequest? PostRequest { get; set; }
    public HttpResponseMessage? Response { get; set; }
    public List<HttpResponseMessage> Responses { get; set; } = new();
    public string? XmlContent { get; set; }
    public string? CorrelationId { get; set; } // Store correlation ID for current scenario
    public string? Traceparent { get; set; } // Store W3C traceparent header for current scenario
    public HttpClient Client => _client;
    public BlogApiFactory Factory => _factory;

    // Cache for response content to allow multiple reads
    private string? _cachedResponseContent;

    // Helper methods
    public async Task<Guid> CreateTestAuthorAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();

        return await AuthorBuilder.DefaultAlbert()
            .BuildAndSaveAsync(dbContext);
    }

    private async Task<string> GetCachedResponseContentAsync()
    {
        if (_cachedResponseContent == null && Response != null)
        {
            _cachedResponseContent = await Response.Content.ReadAsStringAsync();
        }
        return _cachedResponseContent ?? string.Empty;
    }

    public void ClearResponseCache()
    {
        _cachedResponseContent = null;
    }

    public async Task<CreatePostResponse?> GetCreatedPostFromResponse()
    {
        if (Response == null) return null;
        var content = await GetCachedResponseContentAsync();
        return System.Text.Json.JsonSerializer.Deserialize<CreatePostResponse>(content,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<GetPostResponse?> GetPostFromResponse()
    {
        if (Response == null) return null;
        var content = await GetCachedResponseContentAsync();
        return System.Text.Json.JsonSerializer.Deserialize<GetPostResponse>(content,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<HealthCheckResponse?> GetHealthCheckFromResponse()
    {
        if (Response == null) return null;
        var content = await GetCachedResponseContentAsync();
        return System.Text.Json.JsonSerializer.Deserialize<HealthCheckResponse>(content,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public void RemoveApiVersionHeader()
    {
        _client.DefaultRequestHeaders.Remove("X-API-Version");
    }

    public void SetApiVersion(string version)
    {
        _client.DefaultRequestHeaders.Remove("X-API-Version");
        _client.DefaultRequestHeaders.Add("X-API-Version", version);
    }

    public void SetCorrelationId(string correlationId)
    {
        CorrelationId = correlationId;
    }

    public void ClearCorrelationId()
    {
        CorrelationId = null;
    }

    public void SetTraceparent(string traceparent)
    {
        Traceparent = traceparent;
    }

    public void ClearTraceparent()
    {
        Traceparent = null;
    }
}
