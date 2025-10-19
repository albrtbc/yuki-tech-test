# Yuki.Blog.Sdk

Official .NET SDK for the Blog API. Provides a strongly-typed HTTP client for easy integration with the Blog service.

## Features

- Strongly-typed request and response models
- **Automatic correlation ID propagation** for distributed tracing
- Built-in retry logic with Polly for transient failures
- Comprehensive exception handling
- Support for dependency injection
- Configurable via appsettings.json or code
- XML documentation for IntelliSense support
- Header-based API versioning
- Rate limiting support

## Installation

```bash
dotnet add package Yuki.Blog.Sdk
```

## Quick Start

### 1. Configuration via appsettings.json

```json
{
  "BlogClient": {
    "BaseUrl": "https://localhost:5000",
    "ApiVersion": "1.0",
    "TimeoutSeconds": 30,
    "EnableRetry": true,
    "MaxRetryAttempts": 3,
    "EnableCorrelationId": true
  }
}
```

### 2. Register the Client

```csharp
// Program.cs or Startup.cs
using Yuki.Blog.Sdk;

builder.Services.AddBlogClient(builder.Configuration);
```

### 3. Use the Client

```csharp
using Yuki.Blog.Sdk.Interfaces;
using Yuki.Blog.Sdk.Models.Requests;

public class BlogService
{
    private readonly IBlogClient _blogClient;

    public BlogService(IBlogClient blogClient)
    {
        _blogClient = blogClient;
    }

    public async Task CreateBlogPost()
    {
        try
        {
            var request = new CreatePostRequest
            {
                AuthorId = Guid.Parse("your-author-id"),
                Title = "My First Post",
                Description = "A brief description",
                Content = "Full content of the blog post..."
            };

            var response = await _blogClient.CreatePostAsync(request);

            Console.WriteLine($"Created post with ID: {response.Id}");
        }
        catch (BadRequestException ex)
        {
            Console.WriteLine($"Invalid request: {ex.Message}");
        }
        catch (NotFoundException ex)
        {
            Console.WriteLine($"Not found: {ex.Message}");
        }
        catch (RateLimitException ex)
        {
            Console.WriteLine($"Rate limited. Retry after: {ex.RetryAfter}");
        }
    }

    public async Task GetBlogPost(Guid postId)
    {
        try
        {
            // Get post without author info
            var post = await _blogClient.GetPostByIdAsync(postId);

            // Get post with author info
            var postWithAuthor = await _blogClient.GetPostByIdAsync(
                postId,
                includeAuthor: true);

            if (postWithAuthor.Author != null)
            {
                Console.WriteLine($"Author: {postWithAuthor.Author.FullName}");
            }
        }
        catch (NotFoundException ex)
        {
            Console.WriteLine($"Post not found: {ex.Message}");
        }
    }
}
```

## Configuration Options

### Via Code

```csharp
services.AddBlogClient(options =>
{
    options.BaseUrl = "https://api.yourdomain.com";
    options.ApiVersion = "1.0";
    options.TimeoutSeconds = 30;
    options.EnableRetry = true;
    options.MaxRetryAttempts = 3;
    options.EnableCorrelationId = true;  // Enable distributed tracing
});
```

### Configuration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BaseUrl` | string | *required* | The base URL of the Blog API |
| `ApiVersion` | string | "1.0" | API version (sent in X-API-Version header) |
| `TimeoutSeconds` | int | 30 | HTTP request timeout in seconds |
| `EnableRetry` | bool | true | Enable automatic retry on transient failures |
| `MaxRetryAttempts` | int | 3 | Maximum number of retry attempts |
| `EnableCorrelationId` | bool | true | Enable automatic correlation ID propagation for distributed tracing |

## API Reference

### IBlogClient Methods

#### CreatePostAsync

Creates a new blog post.

```csharp
Task<CreatePostResponse> CreatePostAsync(
    CreatePostRequest request,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `request`: The post creation request containing title, description, content, and author ID
- `cancellationToken`: Optional cancellation token

**Returns:** `CreatePostResponse` with the created post details

**Throws:**
- `BadRequestException`: Request validation failed
- `NotFoundException`: Author not found
- `RateLimitException`: Rate limit exceeded
- `BlogApiException`: General API error

#### GetPostByIdAsync

Retrieves a blog post by its unique identifier.

```csharp
Task<GetPostResponse> GetPostByIdAsync(
    Guid id,
    bool includeAuthor = false,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `id`: The unique identifier of the post
- `includeAuthor`: Whether to include author information in the response
- `cancellationToken`: Optional cancellation token

**Returns:** `GetPostResponse` with the post details and optionally author info

**Throws:**
- `NotFoundException`: Post not found
- `BlogApiException`: General API error

## Exception Handling

The SDK provides specific exception types for different error scenarios:

```csharp
try
{
    var post = await _blogClient.GetPostByIdAsync(postId);
}
catch (NotFoundException ex)
{
    // Handle 404 - resource not found
    Console.WriteLine($"Post not found: {ex.Message}");
}
catch (BadRequestException ex)
{
    // Handle 400 - validation error
    Console.WriteLine($"Invalid request: {ex.Message}");
    Console.WriteLine($"Details: {ex.ResponseContent}");
}
catch (RateLimitException ex)
{
    // Handle 429 - rate limit
    Console.WriteLine($"Rate limited. Retry after: {ex.RetryAfter}");
}
catch (BlogApiException ex)
{
    // Handle other API errors
    Console.WriteLine($"API error: {ex.Message}");
    Console.WriteLine($"Status: {ex.StatusCode}");
}
```

## Best Practices

### 1. Use Dependency Injection

Always register the client via DI rather than creating instances manually:

```csharp
// Good
services.AddBlogClient(configuration);

// Avoid
var client = new BlogClient(...);
```

### 2. Handle Exceptions Appropriately

Always wrap SDK calls in try-catch blocks and handle specific exceptions:

```csharp
try
{
    await _blogClient.CreatePostAsync(request);
}
catch (BadRequestException ex)
{
    // Show validation errors to user
}
catch (NotFoundException ex)
{
    // Handle missing resources
}
```

### 3. Use Cancellation Tokens

Pass cancellation tokens for long-running operations:

```csharp
public async Task<IActionResult> GetPost(
    Guid id,
    CancellationToken cancellationToken)
{
    var post = await _blogClient.GetPostByIdAsync(id, false, cancellationToken);
    return Ok(post);
}
```

### 4. Configure Retry Policy

Adjust retry settings based on your requirements:

```csharp
services.AddBlogClient(options =>
{
    options.EnableRetry = true;
    options.MaxRetryAttempts = 5; // Increase for critical operations
});
```

### 5. Correlation ID & Distributed Tracing

The SDK automatically propagates correlation IDs for distributed tracing. When enabled (default), each request includes a `X-Correlation-ID` header containing a unique GUID. This enables request tracking across distributed services.

**Example log output with correlation tracking:**
```
[INFO] CorrelationId=abc-123 | Request started: POST /api/posts
[INFO] CorrelationId=abc-123 | Validating request
[INFO] CorrelationId=abc-123 | Post created with ID: 456
[INFO] CorrelationId=abc-123 | Request completed in 45ms
```

**Disabling correlation ID propagation:**
```csharp
services.AddBlogClient(options =>
{
    options.EnableCorrelationId = false;
});
```

## Architecture

This SDK is designed as a standalone client library following clean architecture principles:

- **Independent**: No dependencies on domain, application, or infrastructure layers
- **HTTP-based**: Communicates via REST API only
- **DTO pattern**: Uses dedicated request/response models
- **SOLID principles**: Interface-based design with dependency injection support

## Development

### Building the SDK

```bash
dotnet build src/Yuki.Blog.Sdk/Yuki.Blog.Sdk.csproj
```

### Creating a NuGet Package

```bash
dotnet pack src/Yuki.Blog.Sdk/Yuki.Blog.Sdk.csproj -c Release
```
