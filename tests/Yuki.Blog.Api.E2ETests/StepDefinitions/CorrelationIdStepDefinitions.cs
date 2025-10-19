using System.Text.RegularExpressions;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace Yuki.Blog.Api.E2ETests.StepDefinitions;

[Binding]
public class CorrelationIdStepDefinitions
{
    private readonly TestContext _context;
    private readonly Dictionary<string, string> _savedCorrelationIds = new();

    public CorrelationIdStepDefinitions(TestContext context)
    {
        _context = context;
    }

    [Given(@"I set X-Correlation-ID header to ""(.*)""")]
    public void GivenISetCorrelationIdHeader(string correlationId)
    {
        _context.SetCorrelationId(correlationId);
    }

    [Given(@"I set traceparent header to ""(.*)""")]
    public void GivenISetTraceparentHeader(string traceparent)
    {
        _context.SetTraceparent(traceparent);
    }

    [Then(@"the response should contain X-Correlation-ID header")]
    public void ThenTheResponseShouldContainCorrelationIdHeader()
    {
        _context.Response.Should().NotBeNull();
        _context.Response!.Headers.Should().ContainKey("X-Correlation-ID");
        _context.Response!.Headers.GetValues("X-Correlation-ID").Should().NotBeEmpty();
    }

    [Then(@"the correlation ID should be a valid GUID")]
    public void ThenTheCorrelationIdShouldBeAValidGuid()
    {
        _context.Response.Should().NotBeNull();
        var correlationIdValues = _context.Response!.Headers.GetValues("X-Correlation-ID");
        correlationIdValues.Should().NotBeEmpty();

        var correlationId = correlationIdValues.First();
        correlationId.Should().NotBeNullOrWhiteSpace();

        // Correlation ID should be a 32-character hex string (OpenTelemetry TraceId format)
        correlationId.Should().MatchRegex(@"^[0-9a-f]{32}$",
            because: $"correlation ID '{correlationId}' should be a valid 32-char hex string (OpenTelemetry TraceId format)");
    }

    [Then(@"the response X-Correlation-ID header should be ""(.*)""")]
    public void ThenTheResponseCorrelationIdHeaderShouldBe(string expectedCorrelationId)
    {
        _context.Response.Should().NotBeNull();
        var correlationIdValues = _context.Response!.Headers.GetValues("X-Correlation-ID");
        correlationIdValues.Should().NotBeEmpty();

        var actualCorrelationId = correlationIdValues.First();
        actualCorrelationId.Should().Be(expectedCorrelationId,
            because: "the server should echo back the correlation ID provided by the client");
    }

    [Then(@"I save the correlation ID as ""(.*)""")]
    public void ThenISaveTheCorrelationIdAs(string key)
    {
        _context.Response.Should().NotBeNull();
        var correlationIdValues = _context.Response!.Headers.GetValues("X-Correlation-ID");
        correlationIdValues.Should().NotBeEmpty();

        var correlationId = correlationIdValues.First();
        _savedCorrelationIds[key] = correlationId;
    }

    [Then(@"the correlation ID should be different from ""(.*)""")]
    public void ThenTheCorrelationIdShouldBeDifferentFrom(string key)
    {
        _context.Response.Should().NotBeNull();
        var correlationIdValues = _context.Response!.Headers.GetValues("X-Correlation-ID");
        correlationIdValues.Should().NotBeEmpty();

        var currentCorrelationId = correlationIdValues.First();
        _savedCorrelationIds.Should().ContainKey(key);

        var savedCorrelationId = _savedCorrelationIds[key];
        currentCorrelationId.Should().NotBe(savedCorrelationId,
            because: "each request without a provided correlation ID should generate a unique one");
    }

    [Then(@"the correlation ID should match OpenTelemetry TraceId format")]
    public void ThenTheCorrelationIdShouldMatchOpenTelemetryTraceIdFormat()
    {
        _context.Response.Should().NotBeNull();
        var correlationIdValues = _context.Response!.Headers.GetValues("X-Correlation-ID");
        correlationIdValues.Should().NotBeEmpty();

        var correlationId = correlationIdValues.First();

        // OpenTelemetry TraceId format: 32 hex characters without hyphens
        var traceIdPattern = @"^[0-9a-f]{32}$";
        Regex.IsMatch(correlationId, traceIdPattern, RegexOptions.IgnoreCase).Should().BeTrue(
            because: $"correlation ID '{correlationId}' should match OpenTelemetry TraceId format (32 hex chars without hyphens)");
    }
}
