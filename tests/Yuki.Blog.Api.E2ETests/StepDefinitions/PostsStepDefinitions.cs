using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Yuki.Blog.Api.Contracts.v1.Requests;
using Yuki.Blog.Api.E2ETests.Builders;
using Yuki.Blog.Api.E2ETests.Helpers;

namespace Yuki.Blog.Api.E2ETests.StepDefinitions;

[Binding]
public class PostsStepDefinitions
{
    private readonly TestContext _context;

    public PostsStepDefinitions(TestContext context)
    {
        _context = context;
    }

    [Given(@"I am an author")]
    public async Task GivenIAmAnAuthor()
    {
        _context.AuthorId = await _context.CreateTestAuthorAsync();
    }

    [Given(@"I have a post with the following details:")]
    public void GivenIHaveAPostWithTheFollowingDetails(Table table)
    {
        var postDetails = table.Rows.ToDictionary(
            row => row["Field"],
            row => row["Value"]
        );

        _context.PostRequest = new CreatePostRequest
        {
            AuthorId = _context.AuthorId ?? Guid.Empty,
            Title = postDetails.GetValueOrDefault("Title", ""),
            Description = postDetails.GetValueOrDefault("Description", ""),
            Content = postDetails.GetValueOrDefault("Content", "")
        };
    }

    [Given(@"I have a non-existent author ID")]
    public void GivenIHaveANonExistentAuthorID()
    {
        _context.AuthorId = Guid.NewGuid();
    }

    [Given(@"I have a post with a title of (.*) characters")]
    public void GivenIHaveAPostWithATitleOfCharacters(int length)
    {
        _context.PostRequest = TestHelpers.CreatePostRequestWithTitleLength(
            _context.AuthorId ?? Guid.Empty,
            length);
    }

    [Given(@"the post has valid description and content")]
    public void GivenThePostHasValidDescriptionAndContent()
    {
        if (_context.PostRequest != null)
        {
            _context.PostRequest = new CreatePostRequest
            {
                AuthorId = _context.PostRequest.AuthorId,
                Title = _context.PostRequest.Title,
                Description = "Valid description",
                Content = "Valid content"
            };
        }
    }

    [Given(@"I have a post with a description of (.*) characters")]
    public void GivenIHaveAPostWithADescriptionOfCharacters(int length)
    {
        _context.PostRequest = TestHelpers.CreatePostRequestWithDescriptionLength(
            _context.AuthorId ?? Guid.Empty,
            length);
    }

    [Given(@"the post has valid title and content")]
    public void GivenThePostHasValidTitleAndContent()
    {
        if (_context.PostRequest != null)
        {
            _context.PostRequest = new CreatePostRequest
            {
                AuthorId = _context.PostRequest.AuthorId,
                Title = "Valid title",
                Description = _context.PostRequest.Description,
                Content = "Valid content"
            };
        }
    }

    [Given(@"I have a post with a content of (.*) characters")]
    public void GivenIHaveAPostWithAContentOfCharacters(int length)
    {
        _context.PostRequest = TestHelpers.CreatePostRequestWithContentLength(
            _context.AuthorId ?? Guid.Empty,
            length);
    }

    [Given(@"the post has valid title and description")]
    public void GivenThePostHasValidTitleAndDescription()
    {
        if (_context.PostRequest != null)
        {
            _context.PostRequest = new CreatePostRequest
            {
                AuthorId = _context.PostRequest.AuthorId,
                Title = "Valid title",
                Description = "Valid description",
                Content = _context.PostRequest.Content
            };
        }
    }

    [Given(@"I have a post request in XML format with title ""(.*)""")]
    public void GivenIHaveAPostRequestInXMLFormatWithTitle(string title)
    {
        _context.XmlContent = TestHelpers.CreateXmlPostRequest(
            _context.AuthorId ?? Guid.Empty,
            title);
    }

    [Given(@"I have a valid post request")]
    public void GivenIHaveAValidPostRequest()
    {
        _context.PostRequest = TestHelpers.CreateValidPostRequest(
            _context.AuthorId ?? Guid.Empty);
    }

    [Given(@"I have created a post with title ""(.*)""")]
    public async Task GivenIHaveCreatedAPostWithTitle(string title)
    {
        var request = new PostBuilder()
            .WithAuthorId(_context.AuthorId ?? Guid.Empty)
            .WithTitle(title)
            .WithDescription("This post was created for integration testing")
            .WithContent("This is the full content of the integration test post.")
            .Build();

        var response = await _context.Client.PostAsJsonAsync("/api/posts", request);
        response.EnsureSuccessStatusCode();

        var createdPost = await response.Content.ReadFromJsonAsync<Yuki.Blog.Api.Contracts.v1.Responses.CreatePostResponse>();
        _context.PostId = createdPost!.Id;
    }

    [Given(@"I have a non-existent post ID")]
    public void GivenIHaveANonExistentPostID()
    {
        _context.PostId = Guid.NewGuid();
    }

    [When(@"I send a POST request to ""(.*)""")]
    public async Task WhenISendAPOSTRequestTo(string endpoint)
    {
        // If correlation ID is set, create a request message with the header
        if (!string.IsNullOrEmpty(_context.CorrelationId))
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(_context.PostRequest)
            };
            request.Headers.Add("X-Correlation-ID", _context.CorrelationId);
            _context.Response = await _context.Client.SendAsync(request);
        }
        else
        {
            _context.Response = await _context.Client.PostAsJsonAsync(endpoint, _context.PostRequest);
        }
    }

    [When(@"I send the XML request to ""(.*)""")]
    public async Task WhenISendTheXMLRequestTo(string endpoint)
    {
        var content = new StringContent(_context.XmlContent!, System.Text.Encoding.UTF8, "application/xml");
        _context.Response = await _context.Client.PostAsync(endpoint, content);
    }

    [When(@"I send (.*) rapid POST requests to ""(.*)""")]
    public async Task WhenISendRapidPOSTRequestsTo(int count, string endpoint)
    {
        _context.Responses.Clear();
        for (int i = 0; i < count; i++)
        {
            var response = await _context.Client.PostAsJsonAsync(endpoint, _context.PostRequest);
            _context.Responses.Add(response);
        }
    }

    [When(@"I send a GET request with Accept header ""(.*)"" to ""(.*)""")]
    public async Task WhenISendAGETRequestWithAcceptHeaderTo(string acceptHeader, string endpoint)
    {
        endpoint = endpoint.Replace("{postId}", _context.PostId?.ToString() ?? "");
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(acceptHeader));
        _context.Response = await _context.Client.SendAsync(request);
    }

    [When(@"I send two consecutive GET requests to ""(.*)""")]
    public async Task WhenISendTwoConsecutiveGETRequestsTo(string endpoint)
    {
        endpoint = endpoint.Replace("{postId}", _context.PostId?.ToString() ?? "");
        _context.Responses.Clear();
        _context.Responses.Add(await _context.Client.GetAsync(endpoint));
        _context.Responses.Add(await _context.Client.GetAsync(endpoint));
    }

    [Then(@"the response should contain a post ID")]
    public async Task ThenTheResponseShouldContainAPostID()
    {
        var createdPost = await _context.GetCreatedPostFromResponse();
        createdPost.Should().NotBeNull();
        createdPost!.Id.Should().NotBeEmpty();
        _context.PostId = createdPost.Id;
    }

    [Then(@"the response should contain the post details")]
    public async Task ThenTheResponseShouldContainThePostDetails()
    {
        if (_context.Response!.StatusCode == HttpStatusCode.Created)
        {
            var createdPost = await _context.GetCreatedPostFromResponse();
            createdPost.Should().NotBeNull();
            createdPost!.AuthorId.Should().Be(_context.AuthorId!.Value);
        }
        else
        {
            var post = await _context.GetPostFromResponse();
            post.Should().NotBeNull();
            post!.Id.Should().Be(_context.PostId!.Value);
        }
    }

    [Then(@"the response should have a Location header with the post URL")]
    public void ThenTheResponseShouldHaveALocationHeaderWithThePostURL()
    {
        _context.Response!.Headers.Location.Should().NotBeNull();
        _context.Response.Headers.Location!.ToString().Should().Contain($"/api/posts/");
    }

    [Then(@"the post title should have (.*) characters")]
    public async Task ThenThePostTitleShouldHaveCharacters(int length)
    {
        var createdPost = await _context.GetCreatedPostFromResponse();
        createdPost.Should().NotBeNull();
        createdPost!.Title.Length.Should().Be(length);
    }

    [Then(@"the response should contain special characters")]
    public async Task ThenTheResponseShouldContainSpecialCharacters()
    {
        var createdPost = await _context.GetCreatedPostFromResponse();
        createdPost.Should().NotBeNull();
        createdPost!.Title.Should().Contain("🚀");
        createdPost.Description.Should().Contain("café");
        createdPost.Content.Should().Contain("€");
    }

    [Then(@"at least some requests should succeed")]
    public void ThenAtLeastSomeRequestsShouldSucceed()
    {
        var successResponses = _context.Responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        successResponses.Should().BeGreaterThan(0, "At least some requests should succeed");
    }

    [Then(@"all requests should complete")]
    public void ThenAllRequestsShouldComplete()
    {
        var successResponses = _context.Responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var rateLimitedResponses = _context.Responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        (successResponses + rateLimitedResponses).Should().Be(_context.Responses.Count, "All requests should complete");
    }

    [Then(@"the author information should not be included by default")]
    public async Task ThenTheAuthorInformationShouldNotBeIncludedByDefault()
    {
        var post = await _context.GetPostFromResponse();
        post.Should().NotBeNull();
        post!.Author.Should().BeNull();
    }

    [Then(@"the author information should be included")]
    public async Task ThenTheAuthorInformationShouldBeIncluded()
    {
        var post = await _context.GetPostFromResponse();
        post.Should().NotBeNull();
        post!.Author.Should().NotBeNull();
    }

    [Then(@"the author should have a name and surname")]
    public async Task ThenTheAuthorShouldHaveANameAndSurname()
    {
        var post = await _context.GetPostFromResponse();
        post!.Author!.Name.Should().NotBeNullOrEmpty();
        post.Author.Surname.Should().NotBeNullOrEmpty();
    }

    [Then(@"both responses should have status code (.*)")]
    public void ThenBothResponsesShouldHaveStatusCode(int statusCode)
    {
        _context.Responses.Should().HaveCount(2);
        _context.Responses[0].StatusCode.Should().Be((HttpStatusCode)statusCode);
        _context.Responses[1].StatusCode.Should().Be((HttpStatusCode)statusCode);
    }

    [Then(@"both responses should have cache control headers")]
    public void ThenBothResponsesShouldHaveCacheControlHeaders()
    {
        _context.Responses[0].Headers.CacheControl.Should().NotBeNull();
        _context.Responses[1].Headers.CacheControl.Should().NotBeNull();
    }
}
