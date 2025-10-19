using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace Yuki.Blog.Api.E2ETests.StepDefinitions;

[Binding]
public class HealthStepDefinitions
{
    private readonly TestContext _context;
    private Stopwatch? _stopwatch;

    public HealthStepDefinitions(TestContext context)
    {
        _context = context;
    }

    [Given(@"I remove the API version header")]
    public void GivenIRemoveTheAPIVersionHeader()
    {
        _context.RemoveApiVersionHeader();
    }

    [When(@"I time a GET request to ""(.*)""")]
    public async Task WhenITimeAGETRequestTo(string endpoint)
    {
        _stopwatch = Stopwatch.StartNew();
        _context.Response = await _context.Client.GetAsync(endpoint);
        _stopwatch.Stop();
    }

    [When(@"I send (.*) concurrent GET requests to ""(.*)""")]
    public async Task WhenISendConcurrentGETRequestsTo(int count, string endpoint)
    {
        _context.Responses.Clear();
        var tasks = Enumerable.Range(0, count)
            .Select(_ => _context.Client.GetAsync(endpoint))
            .ToArray();

        var responses = await Task.WhenAll(tasks);
        _context.Responses.AddRange(responses);
    }

    [Then(@"the health status should be ""(.*)""")]
    public async Task ThenTheHealthStatusShouldBe(string expectedStatus)
    {
        var healthResponse = await _context.GetHealthCheckFromResponse();
        healthResponse.Should().NotBeNull();
        healthResponse!.Status.Should().Be(expectedStatus);
    }

    [Then(@"the total duration should be greater than or equal to (.*)")]
    public async Task ThenTheTotalDurationShouldBeGreaterThanOrEqualTo(int minDuration)
    {
        var healthResponse = await _context.GetHealthCheckFromResponse();
        healthResponse.Should().NotBeNull();
        healthResponse!.TotalDuration.Should().BeGreaterThanOrEqualTo(minDuration);
    }

    [Then(@"the response should contain health checks")]
    public async Task ThenTheResponseShouldContainHealthChecks()
    {
        var healthResponse = await _context.GetHealthCheckFromResponse();
        healthResponse.Should().NotBeNull();
        healthResponse!.Checks.Should().NotBeEmpty();
    }

    [Then(@"the database health check should be present")]
    public async Task ThenTheDatabaseHealthCheckShouldBePresent()
    {
        var healthResponse = await _context.GetHealthCheckFromResponse();
        healthResponse.Should().NotBeNull();

        var databaseCheck = healthResponse!.Checks
            .FirstOrDefault(c => c.Name.Contains("database", StringComparison.OrdinalIgnoreCase) ||
                                c.Name.Contains("npgsql", StringComparison.OrdinalIgnoreCase));

        databaseCheck.Should().NotBeNull("Database health check should be present");
    }

    [Then(@"the database status should be ""(.*)""")]
    public async Task ThenTheDatabaseStatusShouldBe(string expectedStatus)
    {
        var healthResponse = await _context.GetHealthCheckFromResponse();
        var databaseCheck = healthResponse!.Checks
            .FirstOrDefault(c => c.Name.Contains("database", StringComparison.OrdinalIgnoreCase) ||
                                c.Name.Contains("npgsql", StringComparison.OrdinalIgnoreCase));

        databaseCheck.Should().NotBeNull();
        databaseCheck!.Status.Should().Be(expectedStatus);
    }

    [Then(@"the database check duration should be greater than or equal to (.*)")]
    public async Task ThenTheDatabaseCheckDurationShouldBeGreaterThanOrEqualTo(int minDuration)
    {
        var healthResponse = await _context.GetHealthCheckFromResponse();
        var databaseCheck = healthResponse!.Checks
            .FirstOrDefault(c => c.Name.Contains("database", StringComparison.OrdinalIgnoreCase) ||
                                c.Name.Contains("npgsql", StringComparison.OrdinalIgnoreCase));

        databaseCheck.Should().NotBeNull();
        databaseCheck!.Duration.Should().BeGreaterThanOrEqualTo(minDuration);
    }

    [Then(@"the health status should be one of ""(.*)""")]
    public async Task ThenTheHealthStatusShouldBeOneOf(string validStatuses)
    {
        var healthResponse = await _context.GetHealthCheckFromResponse();
        healthResponse.Should().NotBeNull();

        var statusList = validStatuses.Split(',').Select(s => s.Trim()).ToArray();
        healthResponse!.Status.Should().BeOneOf(statusList);
    }

    [Then(@"all health checks should have valid status values")]
    public async Task ThenAllHealthChecksShouldHaveValidStatusValues()
    {
        var healthResponse = await _context.GetHealthCheckFromResponse();
        healthResponse.Should().NotBeNull();

        foreach (var check in healthResponse!.Checks)
        {
            check.Status.Should().NotBeNullOrEmpty();
            check.Status.Should().BeOneOf("Healthy", "Degraded", "Unhealthy");
        }
    }

    [Then(@"all health checks should have non-negative durations")]
    public async Task ThenAllHealthChecksShouldHaveNonNegativeDurations()
    {
        var healthResponse = await _context.GetHealthCheckFromResponse();
        healthResponse.Should().NotBeNull();

        foreach (var check in healthResponse!.Checks)
        {
            check.Duration.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Then(@"the response time should be less than (.*) seconds")]
    public void ThenTheResponseTimeShouldBeLessThanSeconds(int maxSeconds)
    {
        _stopwatch.Should().NotBeNull();
        _stopwatch!.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(maxSeconds));
    }

    [Then(@"all responses should have status code (.*)")]
    public void ThenAllResponsesShouldHaveStatusCode(int statusCode)
    {
        _context.Responses.Should().NotBeEmpty();
        foreach (var response in _context.Responses)
        {
            ((int)response.StatusCode).Should().Be(statusCode);
        }
    }

    [Then(@"all responses should have ""(.*)"" status")]
    public async Task ThenAllResponsesShouldHaveStatus(string expectedStatus)
    {
        _context.Responses.Should().NotBeEmpty();
        foreach (var response in _context.Responses)
        {
            var healthResponse = await response.Content.ReadFromJsonAsync<Yuki.Blog.Api.Contracts.v1.Responses.HealthCheckResponse>();
            healthResponse.Should().NotBeNull();
            healthResponse!.Status.Should().Be(expectedStatus);
        }
    }

    [Then(@"all health checks should have non-empty names")]
    public async Task ThenAllHealthChecksShouldHaveNonEmptyNames()
    {
        var healthResponse = await _context.GetHealthCheckFromResponse();
        healthResponse.Should().NotBeNull();

        foreach (var check in healthResponse!.Checks)
        {
            check.Name.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Then(@"all health checks should have non-empty status values")]
    public async Task ThenAllHealthChecksShouldHaveNonEmptyStatusValues()
    {
        var healthResponse = await _context.GetHealthCheckFromResponse();
        healthResponse.Should().NotBeNull();

        foreach (var check in healthResponse!.Checks)
        {
            check.Status.Should().NotBeNullOrWhiteSpace();
        }
    }
}
