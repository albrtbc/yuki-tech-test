using BoDi;
using TechTalk.SpecFlow;
using Xunit;

namespace Yuki.Blog.Api.E2ETests.StepDefinitions;

/// <summary>
/// Configures SpecFlow dependency injection to work with xUnit fixtures.
/// </summary>
[Binding]
public class ScenarioDependencies : ICollectionFixture<BlogApiCollection>
{
    private static BlogApiFactory? _sharedFactory;
    private static readonly object _lock = new object();

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        lock (_lock)
        {
            if (_sharedFactory == null)
            {
                _sharedFactory = new BlogApiFactory();
                _sharedFactory.InitializeAsync().GetAwaiter().GetResult();
            }
        }
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        if (_sharedFactory != null)
        {
            await _sharedFactory.DisposeAsync();
            _sharedFactory = null;
        }
    }

    [BeforeScenario(Order = 0)]
    public void RegisterDependencies(IObjectContainer container)
    {
        // Register the shared factory if not already registered
        if (_sharedFactory != null && !container.IsRegistered<BlogApiFactory>())
        {
            container.RegisterInstanceAs(_sharedFactory);
        }
    }
}
