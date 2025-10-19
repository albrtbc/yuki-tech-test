using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechTalk.SpecFlow;
using Xunit;
using Yuki.Blog.Infrastructure.Persistence;

namespace Yuki.Blog.Api.E2ETests.StepDefinitions;

/// <summary>
/// SpecFlow hooks for setting up and tearing down test contexts.
/// </summary>
[Binding]
public class Hooks : IClassFixture<BlogApiFactory>, ICollectionFixture<BlogApiCollection>
{
    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        // Global setup before all tests
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        // Global teardown after all tests
    }

    [BeforeScenario(Order = 1)]
    public async Task BeforeScenario(TestContext testContext)
    {
        // Clean database before each scenario to ensure test isolation
        await CleanDatabaseAsync(testContext.Factory);
    }

    [AfterScenario]
    public void AfterScenario(TestContext testContext)
    {
        // Cleanup after each scenario
        testContext.Response?.Dispose();

        foreach (var response in testContext.Responses)
        {
            response?.Dispose();
        }

        testContext.Responses.Clear();
        testContext.ClearResponseCache();

        // Clear correlation ID for next scenario
        testContext.ClearCorrelationId();
    }

    /// <summary>
    /// Cleans all test data from the database to ensure test isolation.
    /// </summary>
    private static async Task CleanDatabaseAsync(BlogApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();

        // Remove all test data using ExecuteDelete (Posts must be deleted before Authors due to FK constraint)
        // Use lowercase table names for PostgreSQL
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"posts\", \"authors\" CASCADE");
    }
}
