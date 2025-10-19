# Yuki Blog - Backend Technical Test

A production-ready RESTful API for a blogging system built with **Clean Architecture**, **Domain-Driven Design (DDD)**, and **CQRS** using **.NET 8**.

## 🏗️ Tech Stack & Architecture

- **Clean Architecture** with strict dependency inversion
- **Domain-Driven Design** - Rich domain models, value objects, aggregates, domain events
- **CQRS** with MediatR
- **Result Pattern** for error handling
- **Repository Pattern** with Unit of Work
- **Entity Framework Core 8** with PostgreSQL
- **Strongly-Typed IDs** for compile-time safety
- **Header-Based API Versioning**
- **Docker & Docker Compose**
- **Structured Logging** with Serilog + Seq log aggregation
- **OpenTelemetry Distributed Tracing** with automatic instrumentation
- **Correlation ID** using OpenTelemetry TraceId
- **Health Checks** with database monitoring
- **Rate Limiting** and **Response Caching**
- **JSON/XML Support**

### Why NO Event Sourcing?

Despite Event Sourcing being "valued" in the requirements, we **pragmatically chose NOT to implement it**. A simple CRUD blog doesn't need event sourcing complexity. **Knowing when NOT to use a pattern is as important as knowing how to use it.**

We **did implement Domain Events** (in-memory notifications) which is the appropriate DDD tactical pattern for this use case.

---

## 🚀 Quick Start

### Prerequisites

**All Platforms:**
- [Docker](https://docs.docker.com/get-docker/) - Container platform
- [Docker Compose](https://docs.docker.com/compose/install/) - Multi-container orchestration (included with Docker Desktop)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) - Required for local development

**Windows (PowerShell):**
- [Chocolatey](https://chocolatey.org/install) - Package manager for Windows
- Make - Install via Chocolatey: `choco install make`

**WSL (Windows Subsystem for Linux):**
- Make is typically pre-installed. If not: `sudo apt-get install build-essential`

**macOS/Linux:**
- Make is typically pre-installed

### Using Make (Recommended)

```bash
git clone <repository-url>
cd yuki-tech-test

make help        # View all commands
make run         # Build, start DB, and run API
```

**Swagger:** http://localhost:5000/swagger

**Common Commands:**
```bash
make test          # Run tests with coverage
make docker-up     # Start all services with Docker Compose
make docker-down   # Stop services
```

### Using Docker

```bash
docker compose up --build
```

**Services:**
- Swagger: http://localhost:8080/swagger
- PostgreSQL: localhost:5432
- Seq (Logs): http://localhost:5341

---

## 📡 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/posts` | Create a blog post (rate-limited: 10/min) |
| GET | `/api/posts/{id}` | Get a post by ID (cached: 60s) |
| GET | `/api/posts/{id}?include=author` | Get post with author (cached: 60s) |
| GET | `/api/health` | Health check with database status (cached: 10s) |

**API Versioning:** Use `X-API-Version: 1.0` header

📘 **Full API documentation:** [Swagger UI](http://localhost:5000/swagger) (5000 for `make run`, 8080 for Docker)

---

## 🏛️ Project Structure

```
├── src/
│   ├── Yuki.Blog.Domain/          # Core domain (entities, value objects, events)
│   ├── Yuki.Blog.Application/     # Use cases (commands, queries, handlers)
│   ├── Yuki.Blog.Infrastructure/  # EF Core, repositories, services
│   ├── Yuki.Blog.Api/             # Controllers, middleware, filters
│   └── Yuki.Blog.Sdk/             # Official .NET SDK (HTTP client)
├── tests/
│   ├── Yuki.Blog.Domain.UnitTests/
│   ├── Yuki.Blog.Application.UnitTests/
│   ├── Yuki.Blog.Infrastructure.UnitTests/
│   ├── Yuki.Blog.Sdk.UnitTests/
│   └── Yuki.Blog.Api.E2ETests/
├── docker-compose.yml
└── Makefile
```

**Clean Architecture Layers:** API → Application (CQRS) → Domain → Infrastructure

**Design Patterns:** CQRS, Repository, Unit of Work, Result Pattern, Domain Events, Factory, Strategy, Dependency Injection

---

## 🧪 Testing

### Running Tests

```bash
make test           # Run all tests (unit + E2E) with coverage
make e2e            # Run all E2E tests
make e2e-smoke      # Run smoke tests only (critical path)
make e2e-critical   # Run all critical tests
```

### E2E Testing with SpecFlow BDD

The E2E test suite uses **SpecFlow** (Behavior-Driven Development) with **Gherkin** syntax for business-readable test scenarios.

**Key Features:**
- **Gherkin Syntax** - Tests written in Given-When-Then format (`.feature` files)
- **TestContainers** - Isolated PostgreSQL database for each test run
- **Test Data Builders** - Fluent API with Bogus for realistic fake data
- **Database Cleanup** - Automatic truncation before each scenario for test isolation
- **Test Tags** - Filter tests by category (@smoke, @critical, @boundary, etc.)

**Test Categories:**
- `@smoke` - Critical path tests (5 scenarios)
- `@critical` - All essential tests (5 scenarios)
- `@boundary` - Boundary value testing
- `@validation` - Input validation tests
- `@error-handling` - Error scenario tests
- `@encoding` - UTF-8 and special character tests
- `@performance` - Rate limiting and concurrency tests

**Example Feature File:**
```gherkin
Feature: Blog Posts Management
  As a blog user
  I want to create and retrieve blog posts
  So that I can share and access my content

  @smoke @critical
  Scenario: Create a valid blog post
    Given I am an author
    And I have a post with the following details:
      | Field       | Value                    |
      | Title       | Test Blog Post           |
      | Description | This is a test           |
      | Content     | Full content here        |
    When I send a POST request to "/api/posts"
    Then the response status code should be 201
    And the response should contain a post ID
```

All tests run with code coverage enabled. E2E tests use TestContainers for isolated database testing with minimal logging.

**Coverage Target:** >90%

---

## 📦 Official .NET SDK

An official .NET SDK (`Yuki.Blog.Sdk`) is provided for easy integration with the Blog API.

### Features
- ✅ Strongly-typed HTTP client with DTOs
- ✅ **OpenTelemetry W3C Trace Context support** - automatic distributed tracing when OpenTelemetry is configured
- ✅ Built-in retry logic with Polly (exponential backoff)
- ✅ Comprehensive exception handling (NotFoundException, BadRequestException, RateLimitException)
- ✅ Dependency injection support
- ✅ Configurable via appsettings.json or code

### Quick Start

```csharp
// 1. Configure in appsettings.json
{
  "BlogClient": {
    "BaseUrl": "https://localhost:5000",
    "ApiVersion": "1.0",
    "EnableRetry": true,
    "MaxRetryAttempts": 3
  }
}

// 2. Register in DI
using Yuki.Blog.Sdk;
services.AddBlogClient(configuration);

// 3. (Optional) Add OpenTelemetry for distributed tracing
services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddHttpClientInstrumentation()  // Enables automatic trace propagation
        .AddOtlpExporter());

// 4. Use the client
public class MyService
{
    private readonly IBlogClient _client;

    public MyService(IBlogClient client) => _client = client;

    public async Task CreatePost()
    {
        var post = await _client.CreatePostAsync(new CreatePostRequest
        {
            Title = "Hello",
            Content = "World"
        });
        // TraceId automatically flows from your app → SDK → Blog API
    }
}
```

**Full Documentation:** [SDK README](src/Yuki.Blog.Sdk/README.md)

**Architecture:** The SDK follows Clean Architecture principles - it communicates via HTTP only and has zero dependencies on Domain/Application/Infrastructure layers. Perfect for other teams to consume the API.

**Distributed Tracing:** When you configure OpenTelemetry in your application with `.AddHttpClientInstrumentation()`, the SDK automatically propagates W3C Trace Context (traceparent header) to the Blog API. No SDK configuration needed - it just works!

---

## 🗄️ Database

**PostgreSQL** with Entity Framework Core using `EnsureCreated()` for development.

The database schema is automatically created on startup when running in Development mode. No migrations are required.

**Seed Data:** Three test authors are automatically populated on initialization for test porpuses:
- Albert Blanco (`00000000-0000-0000-0000-000000000001`)
- Test Two (`00000000-0000-0000-0000-000000000002`)
- TestData Three (`00000000-0000-0000-0000-000000000003`)

**Tables:** `posts` (with indexes on author_id, created_at), `authors`

---

## 🔍 Monitoring & Observability

### Health Check
`GET /api/health` - Returns database connection status (JSON/XML format)
- **Response cached for 10 seconds** to reduce database load from frequent health probes
- Checks database connectivity and overall application health
- Returns HTTP 200 for healthy status, HTTP 503 for unhealthy/degraded status

### OpenTelemetry Distributed Tracing
**Complete observability** with OpenTelemetry automatic instrumentation:

**Instrumented Layers:**
- 🌐 **HTTP Requests** - ASP.NET Core automatic instrumentation with client IP and status codes
- 🚀 **MediatR Commands/Queries** - Custom spans for CQRS operations with request metadata
- 📦 **Domain Event Handlers** - Individual spans for each event handler execution
- 🗄️ **Repository Operations** - Business-context spans with entity IDs and operation types
- 💾 **Database Queries** - Entity Framework Core spans with parsed SQL operation names

**Trace Hierarchy Example:**
```
POST /api/posts (200ms)
  └─ MediatR.CreatePostCommand (180ms)
      ├─ Repository.AddPost (52ms)
      ├─ DomainEvent.PostCreatedEvent.LogHandler (8ms)
      ├─ DomainEvent.PostCreatedEvent.AuditHandler (6ms)
      ├─ DomainEvent.PostCreatedEvent.UpdateStatisticsHandler (15ms)
      └─ INSERT posts (11ms)
```

**Key Features:**
- ✅ **W3C Trace Context** standard for distributed tracing
- ✅ **Automatic context propagation** across service boundaries
- ✅ **Custom tags** for business metrics (post.id, author.id, operation names)
- ✅ **Error tracking** with exception details and status codes
- ✅ **Performance metrics** with duration tracking at each layer
- ✅ **OTLP export** to Seq for unified logs and traces

### Logging & Log Aggregation
**Structured Logging** with Serilog writing to multiple sinks:
- **Console** - Formatted output with TraceId/CorrelationId
- **File** - Daily rolling files in `logs/` directory
- **Seq** - Centralized log aggregation with searchable UI and trace correlation

**Seq Observability Platform** (http://localhost:5341):
- 📊 **Unified logs and traces** - Correlate logs with distributed traces
- 🔍 **TraceId correlation** - Click a log to see the complete trace timeline
- 📈 **Real-time monitoring** - Live tail logs and trace spans
- 🎯 **Advanced filtering** - Filter by TraceId, Level, properties, spans, etc.
- 📝 **SQL-like queries** - Powerful query language for log and trace analysis
- 🚨 **Alerting** - Set up alerts based on log patterns or trace metrics
- 📉 **Performance analysis** - Identify slow operations and bottlenecks

**Quick Start:**
```bash
# Start Seq container
docker compose up -d seq

# Access Seq UI
open http://localhost:5341

# View traces: Navigate to "Events" → Click any log → View "Trace" tab
```

**Example Queries in Seq:**
```sql
-- Find all requests slower than 100ms
select * from stream where @Properties.Duration > 100

-- Find all failed database operations
select * from stream where repository.success = false

-- Trace a specific request by correlation ID
select * from stream where CorrelationId = 'abc-123-...'
```

All logs include TraceId (as CorrelationId) for seamless correlation between logs and distributed traces, plus enrichment with machine name and thread ID.

---

## 🔗 Correlation ID & Distributed Tracing

Every request is automatically tracked with a **correlation ID** that is unified with **OpenTelemetry's TraceId** for seamless correlation between logs and traces.

**How it works:**
- OpenTelemetry automatically generates a unique TraceId for each request (W3C Trace Context standard)
- The TraceId is converted to GUID format and exposed as `X-Correlation-ID` in response headers
- All logs include the CorrelationId (which is the TraceId) for request tracking
- All traces use the same TraceId for distributed tracing
- **Single source of truth:** One ID across logs, traces, and client responses

**Key Benefits:**
- ✅ **Unified Observability** - Same ID in logs AND traces in Seq
- ✅ **Industry Standard** - W3C Trace Context for interoperability
- ✅ **Automatic Propagation** - TraceId flows across service boundaries
- ✅ **Client Integration** - Clients can reference the correlation ID for support tickets
- ✅ **Zero Duplication** - No separate correlation ID and trace ID

**Usage Example:**
```bash
# Make a request
curl http://localhost:5000/api/posts/{id}

# Response headers include the TraceId as X-Correlation-ID
# X-Correlation-ID: 4bf92f35-77b3-4da6-a3ce-929d0e0e4736
```

**Log Output:**
```
2025-01-15 10:30:45 [INF] [CorrelationId: 4bf92f35-77b3-4da6-a3ce-929d0e0e4736] HTTP GET /api/posts/123 responded 200
2025-01-15 10:30:45 [INF] [CorrelationId: 4bf92f35-77b3-4da6-a3ce-929d0e0e4736] Retrieved post with ID: 123
```

**Trace View in Seq:**
```
TraceId: 4bf92f35-77b3-4da6-a3ce-929d0e0e4736
  └─ HTTP GET /api/posts/123 (45ms)
      └─ MediatR.GetPostByIdQuery (38ms)
          └─ SELECT posts (12ms)
```

**Correlation in Action:**
1. Client receives `X-Correlation-ID: 4bf92f35-...` in response
2. Client reports issue referencing this correlation ID
3. Search Seq logs by `CorrelationId = '4bf92f35-...'`
4. Click any log entry → View "Trace" tab → See complete distributed trace
5. Identify exact bottleneck or error in the trace timeline

**Architecture:**
- `CorrelationIdMiddleware.cs` extracts OpenTelemetry's `Activity.Current.TraceId`
- Serilog enrichment adds TraceId to all log properties
- OpenTelemetry exports traces to Seq via OTLP protocol
- Seq automatically correlates logs and traces by TraceId

This provides **complete end-to-end observability** from client request to database query.

---

## 🚧 Intentionally Not Implemented

This is a technical demonstration focusing on **architecture quality over feature completeness**. The following features were deliberately left out of scope:

### Authentication & Authorization
- ❌ No JWT/OAuth authentication
- ❌ No user management
- ❌ No role-based access control (RBAC)
- ❌ No API key authentication

**Rationale:** Authentication adds complexity that would distract from demonstrating Clean Architecture, DDD, and CQRS patterns. In production, this would use ASP.NET Core Identity or an external identity provider (Auth0, Keycloak, etc.).

### GraphQL
- ❌ No GraphQL API (using REST with selective field inclusion instead)

**Rationale:** We implemented the `?include=author` query parameter pattern for selective field inclusion rather than adopting GraphQL. Here's why:

**Why NOT GraphQL:**
- **Overkill for simple use cases** - This blog API has straightforward resource relationships. GraphQL shines with complex, deeply nested data graphs.
- **Added complexity** - Would require HotChocolate/GraphQL.NET, schema definitions, resolvers, N+1 query problem mitigation, etc.
- **Caching challenges** - HTTP caching (which we use) is trivial with REST GET requests but complex with GraphQL's POST-based queries.
- **Tooling overhead** - REST works out-of-the-box with Swagger, standard HTTP clients, and browser testing. GraphQL requires specialized tooling.

**Why REST with `?include=author` is sufficient:**
- **Simple and explicit** - Clear what data is returned based on query parameters
- **HTTP caching friendly** - Different URLs = different cache keys (see our 60s cache on `?include=author`)
- **Standard tooling** - Works with all REST clients, curl, Postman, browser, etc.
- **Predictable performance** - We control exactly what queries run (no arbitrary depth queries)
- **Easier testing** - Standard HTTP assertions in E2E tests

**When GraphQL would make sense:**
- Multiple consuming clients with different data needs
- Complex nested relationships (posts → comments → replies → users → posts...)
- Frequent schema changes requiring flexibility
- Mobile apps needing to minimize payload size

For this blog API, REST + selective inclusion is the pragmatic choice. **Use the simplest solution that meets the requirements.**

### Advanced Features
- ❌ No pagination/filtering/sorting on list endpoints
- ❌ No full-text search
- ❌ No file upload (images for posts)
- ❌ No comments system
- ❌ No post categories/tags
- ❌ No soft deletes
- ❌ No audit trails (created by, modified by)

**Rationale:** These are production features that don't demonstrate additional architectural patterns beyond what's already implemented.

### Production Readiness
- ❌ No database migrations (using `EnsureCreated()` for simplicity)
- ❌ No comprehensive input sanitization (XSS protection)
- ❌ No API throttling per user (only IP-based rate limiting)
- ❌ No distributed caching (Redis)
- ❌ No background job processing (Hangfire/Quartz)

**Rationale:** This is a demo/technical test environment. Production deployment would require proper migration strategy, security hardening, and infrastructure setup.

### What WAS Prioritized Instead
✅ **Clean Architecture** - Proper dependency flow and layer separation
✅ **Domain-Driven Design** - Rich domain models, value objects, aggregates, domain events
✅ **CQRS** - Separate read/write models with optimized queries
✅ **Testability** - Comprehensive unit and E2E tests with >90% coverage
✅ **Observability** - OpenTelemetry distributed tracing, structured logging with Serilog, unified TraceId/CorrelationId, Seq integration
✅ **API Design** - Proper REST conventions, versioning, content negotiation
✅ **Code Quality** - SOLID principles, design patterns, XML documentation
