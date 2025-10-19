using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Yuki.Blog.Sdk;
using Yuki.Blog.Sdk.Configuration;

namespace Yuki.Blog.Sdk.UnitTests.Helpers;

/// <summary>
/// Test fixture for creating BlogClient instances with mocked HTTP responses.
/// </summary>
public class BlogClientTestFixture : IDisposable
{
    public MockHttpMessageHandler MockHttp { get; }
    public HttpClient HttpClient { get; }
    public BlogClientOptions Options { get; }
    public JsonSerializerOptions JsonOptions { get; }

    public BlogClientTestFixture()
    {
        MockHttp = new MockHttpMessageHandler();
        HttpClient = MockHttp.ToHttpClient();
        HttpClient.BaseAddress = new Uri("https://api.test.com");

        Options = new BlogClientOptions
        {
            BaseUrl = "https://api.test.com",
            ApiVersion = "1.0",
            TimeoutSeconds = 30
        };

        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Creates a BlogClient instance with the mocked HTTP client.
    /// </summary>
    public BlogClient CreateClient()
    {
        return new BlogClient(HttpClient, Microsoft.Extensions.Options.Options.Create(Options));
    }

    /// <summary>
    /// Sets up a mock HTTP response for a specific request.
    /// </summary>
    public MockedRequest SetupRequest(HttpMethod method, string url)
    {
        return MockHttp.When(method, url);
    }

    /// <summary>
    /// Sets up a successful JSON response.
    /// </summary>
    public void SetupSuccessResponse<T>(HttpMethod method, string url, T responseData)
    {
        var json = JsonSerializer.Serialize(responseData, JsonOptions);
        MockHttp.When(method, url)
            .Respond("application/json", json);
    }

    /// <summary>
    /// Sets up an error response with a specific status code.
    /// </summary>
    public void SetupErrorResponse(HttpMethod method, string url, HttpStatusCode statusCode, string? content = null)
    {
        var request = MockHttp.When(method, url);
        request.Respond(statusCode, "application/json", content ?? string.Empty);
    }

    /// <summary>
    /// Sets up a rate limit response with RetryAfter header.
    /// </summary>
    public void SetupRateLimitResponse(HttpMethod method, string url, TimeSpan retryAfter)
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(retryAfter);

        MockHttp.When(method, url)
            .Respond(_ => response);
    }

    /// <summary>
    /// Gets the number of times a request was matched.
    /// </summary>
    public int GetMatchCount(HttpMethod method, string url)
    {
        return MockHttp.GetMatchCount(MockHttp.When(method, url));
    }

    /// <summary>
    /// Sets up a mock that throws an exception.
    /// </summary>
    public void SetupThrowException(HttpMethod method, string url, Exception exception)
    {
        MockHttp.When(method, url).Throw(exception);
    }

    /// <summary>
    /// Sets up a custom response handler.
    /// </summary>
    public void SetupCustomResponse(HttpMethod method, string url, HttpResponseMessage response)
    {
        MockHttp.When(method, url).Respond(_ => response);
    }

    /// <summary>
    /// Sets up a mock that responds with specific content.
    /// </summary>
    public void SetupContentResponse(HttpMethod method, string url, string contentType, string content)
    {
        MockHttp.When(method, url).Respond(contentType, content);
    }

    public void Dispose()
    {
        HttpClient?.Dispose();
        MockHttp?.Dispose();
        GC.SuppressFinalize(this);
    }
}
