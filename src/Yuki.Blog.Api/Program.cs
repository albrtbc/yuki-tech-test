using System.Threading.RateLimiting;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Yuki.Blog.Api.Middleware;
using Yuki.Blog.Api.Mapping;
using Yuki.Blog.Application;
using Yuki.Blog.Infrastructure;
using Yuki.Blog.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
// Note: WriteTo sinks are configured in appsettings.json to avoid duplication
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .CreateLogger();

builder.Host.UseSerilog();

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "Yuki.Blog.Api",
            serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"))
    .WithTracing(tracing => tracing
        // Add ASP.NET Core instrumentation to trace HTTP requests
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.EnrichWithHttpRequest = (activity, httpRequest) =>
            {
                activity.SetTag("http.request.client_ip", httpRequest.HttpContext.Connection.RemoteIpAddress?.ToString());
            };
            options.EnrichWithHttpResponse = (activity, httpResponse) =>
            {
                activity.SetTag("http.response.status_code", httpResponse.StatusCode);
            };
        })
        // Add Entity Framework Core instrumentation to trace database operations
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true; // Include SQL query text in spans
            options.SetDbStatementForStoredProcedure = true;
            options.EnrichWithIDbCommand = (activity, command) =>
            {
                // Add custom tags for better filtering
                activity.SetTag("db.operation", command.CommandType.ToString());

                // Parse and set a better display name from the SQL command
                var commandText = command.CommandText?.Trim() ?? "";
                if (commandText.Length > 0)
                {
                    // Extract operation type (SELECT, INSERT, UPDATE, DELETE)
                    var operation = commandText.Split(' ')[0].ToUpper();

                    // Try to extract table name
                    string? tableName = null;
                    if (operation == "SELECT")
                    {
                        // Look for FROM clause
                        var fromIndex = commandText.IndexOf("FROM", StringComparison.OrdinalIgnoreCase);
                        if (fromIndex > 0)
                        {
                            var afterFrom = commandText.Substring(fromIndex + 5).Trim();
                            tableName = afterFrom.Split(new[] { ' ', '\n', '\r', '(' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                        }
                    }
                    else if (operation == "INSERT")
                    {
                        // Look for INTO clause
                        var intoIndex = commandText.IndexOf("INTO", StringComparison.OrdinalIgnoreCase);
                        if (intoIndex > 0)
                        {
                            var afterInto = commandText.Substring(intoIndex + 5).Trim();
                            tableName = afterInto.Split(new[] { ' ', '\n', '\r', '(' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                        }
                    }
                    else if (operation == "UPDATE")
                    {
                        // Table name comes right after UPDATE
                        var parts = commandText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 1)
                        {
                            tableName = parts[1];
                        }
                    }
                    else if (operation == "DELETE")
                    {
                        // Look for FROM clause
                        var fromIndex = commandText.IndexOf("FROM", StringComparison.OrdinalIgnoreCase);
                        if (fromIndex > 0)
                        {
                            var afterFrom = commandText.Substring(fromIndex + 5).Trim();
                            tableName = afterFrom.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                        }
                    }

                    // Clean up table name (remove quotes, schema prefix, etc.)
                    if (!string.IsNullOrEmpty(tableName))
                    {
                        tableName = tableName.Trim('"', '\'', '[', ']');
                        // Remove schema prefix if present (e.g., dbo.posts -> posts)
                        if (tableName.Contains('.'))
                        {
                            tableName = tableName.Split('.').Last();
                        }

                        // Set a more descriptive display name
                        activity.DisplayName = $"{operation} {tableName}";
                        activity.SetTag("db.table", tableName);
                    }
                    else
                    {
                        activity.DisplayName = $"{operation} yukiblog";
                    }
                }
            };
        })
        // Add HttpClient instrumentation to trace outgoing HTTP calls to external services
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
            options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
            {
                activity.SetTag("http.request.url", httpRequestMessage.RequestUri?.ToString());
            };
            options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
            {
                activity.SetTag("http.response.status_code", (int)httpResponseMessage.StatusCode);
            };
        })
        // Add our custom MediatR instrumentation from LoggingBehavior
        .AddSource("Yuki.Blog.Application.MediatR")
        // Add our custom Repository instrumentation from TracedRepository
        .AddSource("Yuki.Blog.Infrastructure.Repositories")
        // Export to Seq via OTLP (Seq 2022.1+ natively supports OpenTelemetry)
        .AddOtlpExporter(options =>
        {
            // Seq OTLP HTTP endpoint - Seq auto-detects OTLP on its main port
            // The path /ingest/otlp/v1/traces is handled automatically by the OTLP exporter
            options.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/traces");
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
        })
        // Also keep console exporter for development debugging
        .AddConsoleExporter());

// Add services to the container
builder.Services.AddControllers()
    .AddXmlSerializerFormatters();

// Register API services
builder.Services.AddScoped<IResultMapper, ResultMapper>();

// Add Application layer services
builder.Services.AddApplication();

// Add Infrastructure layer services
builder.Services.AddInfrastructure(builder.Configuration);

// Add Rate Limiting
var rateLimitWindowSeconds = builder.Configuration.GetValue<int>("RateLimiting:FixedWindow:WindowSeconds", 60);
var rateLimitPermitLimit = builder.Configuration.GetValue<int>("RateLimiting:FixedWindow:PermitLimit", 10);
var rateLimitQueueLimit = builder.Configuration.GetValue<int>("RateLimiting:FixedWindow:QueueLimit", 2);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Fixed window rate limiter for POST operations
    options.AddPolicy("fixed", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromSeconds(rateLimitWindowSeconds),
                PermitLimit = rateLimitPermitLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = rateLimitQueueLimit
            }));
});

// Add Response Caching
builder.Services.AddResponseCaching();

// Add API Versioning (header-based)
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = new HeaderApiVersionReader("X-API-Version");
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Yuki Blog API",
        Version = "v1",
        Description = "A RESTful API for a blogging system built with Clean Architecture, DDD, and CQRS",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Yuki Team",
        }
    });

    // Add X-API-Version header parameter
    options.OperationFilter<Yuki.Blog.Api.Filters.ApiVersionHeaderOperationFilter>();

    // Add XML schema filter for correct XML serialization
    options.SchemaFilter<Yuki.Blog.Api.Filters.XmlSchemaFilter>();

    // Add query parameters schema filter to remove computed properties
    options.SchemaFilter<Yuki.Blog.Api.Filters.QueryParametersSchemaFilter>();

    // Add example values schema filter for Swagger UI examples
    options.SchemaFilter<Yuki.Blog.Api.Filters.ExampleValuesSchemaFilter>();

    // Include XML comments if they exist
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add FluentValidation rules to Swagger documentation
builder.Services.AddFluentValidationRulesToSwagger();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

// Add CORS (configure as needed)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Yuki Blog API v1");

        // Pre-populate X-API-Version header with default value using request interceptor
        options.UseRequestInterceptor("(request) => { " +
            "if (!request.headers['X-API-Version']) { " +
            "request.headers['X-API-Version'] = '1.0'; " +
            "} " +
            "return request; " +
            "}");
    });

    // Apply migrations automatically in Development
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();

    // Use EnsureCreated only in Development for quick setup
    // In production, migrations should be applied separately via deployment pipeline
    if (app.Environment.IsDevelopment())
    {
        dbContext.Database.EnsureCreated();
    }
    else
    {
        // In non-development environments, apply pending migrations
        dbContext.Database.Migrate();
    }
}

// Security headers middleware - must be early in pipeline
app.UseSecurityHeaders();

// Correlation ID middleware - must be first to ensure all logs have correlation ID
app.UseCorrelationId();

// Serilog request logging - must be after CorrelationId to include it in logs
// This replaces ASP.NET Core's default request logging
app.UseSerilogRequestLogging(options =>
{
    // Enrich log events with additional properties from the diagnostic context
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        // The CorrelationId is already set by our middleware via IDiagnosticContext
        // This ensures it's included in the Serilog request completion log
    };
});

// Global exception handling middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors();

// Enable rate limiting middleware
app.UseRateLimiter();

// Enable response caching middleware
app.UseResponseCaching();

app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting Yuki Blog API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make the Program class public for testing
public partial class Program { }
