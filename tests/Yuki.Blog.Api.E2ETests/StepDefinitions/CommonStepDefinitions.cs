using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace Yuki.Blog.Api.E2ETests.StepDefinitions;

[Binding]
public class CommonStepDefinitions
{
    private readonly TestContext _context;

    public CommonStepDefinitions(TestContext context)
    {
        _context = context;
    }

    [Given(@"the API version is ""(.*)""")]
    public void GivenTheAPIVersionIs(string version)
    {
        _context.SetApiVersion(version);
    }

    [When(@"I send a GET request to ""(.*)""")]
    public async Task WhenISendAGETRequestTo(string endpoint)
    {
        _context.ClearResponseCache();
        endpoint = endpoint.Replace("{postId}", _context.PostId?.ToString() ?? "");

        // If correlation ID or traceparent is set, use a request message with the header
        if (!string.IsNullOrEmpty(_context.CorrelationId) || !string.IsNullOrEmpty(_context.Traceparent))
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

            if (!string.IsNullOrEmpty(_context.CorrelationId))
            {
                request.Headers.Add("X-Correlation-ID", _context.CorrelationId);
            }

            if (!string.IsNullOrEmpty(_context.Traceparent))
            {
                request.Headers.Add("traceparent", _context.Traceparent);
            }

            _context.Response = await _context.Client.SendAsync(request);
        }
        else
        {
            _context.Response = await _context.Client.GetAsync(endpoint);
        }
    }

    [When(@"I send a POST request to ""(.*)"" with body:")]
    public async Task WhenISendAPOSTRequestToWithBody(string endpoint, string body)
    {
        _context.ClearResponseCache();

        // Parse the JSON body
        var jsonDocument = JsonDocument.Parse(body);
        var content = JsonContent.Create(jsonDocument.RootElement);

        // If correlation ID is set, use a request message with the header
        if (!string.IsNullOrEmpty(_context.CorrelationId))
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };
            request.Headers.Add("X-Correlation-ID", _context.CorrelationId);
            _context.Response = await _context.Client.SendAsync(request);
        }
        else
        {
            _context.Response = await _context.Client.PostAsync(endpoint, content);
        }
    }

    [Then(@"the response status code should be (.*)")]
    public void ThenTheResponseStatusCodeShouldBe(int statusCode)
    {
        _context.Response.Should().NotBeNull();
        ((int)_context.Response!.StatusCode).Should().Be(statusCode);
    }

    [Then(@"the response content type should be ""(.*)""")]
    public void ThenTheResponseContentTypeShouldBe(string contentType)
    {
        _context.Response!.Content.Headers.ContentType?.MediaType.Should().Be(contentType);
    }
}
