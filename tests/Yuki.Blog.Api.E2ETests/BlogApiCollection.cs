using Xunit;

namespace Yuki.Blog.Api.E2ETests;

/// <summary>
/// Defines a test collection that shares a single BlogApiFactory instance
/// across all test classes, reducing container startup overhead.
/// </summary>
[CollectionDefinition(nameof(BlogApiCollection))]
public class BlogApiCollection : ICollectionFixture<BlogApiFactory>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
