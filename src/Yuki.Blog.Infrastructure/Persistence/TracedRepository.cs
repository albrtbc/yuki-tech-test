using System.Diagnostics;

namespace Yuki.Blog.Infrastructure.Persistence;

/// <summary>
/// Base class for repositories that provides OpenTelemetry tracing capabilities.
/// Wraps repository operations with Activity spans for distributed tracing.
/// </summary>
public abstract class TracedRepository
{
    private static readonly ActivitySource ActivitySource = new("Yuki.Blog.Infrastructure.Repositories");

    /// <summary>
    /// Executes an asynchronous repository operation with OpenTelemetry tracing.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operationName">The name of the repository operation (e.g., "GetPostById").</param>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="enrichActivity">Optional action to add custom tags to the activity.</param>
    /// <returns>The result of the operation.</returns>
    protected async Task<T> TraceAsync<T>(
        string operationName,
        Func<Task<T>> operation,
        Action<Activity?>? enrichActivity = null)
    {
        using var activity = ActivitySource.StartActivity($"Repository.{operationName}", ActivityKind.Internal);

        // Add common tags
        activity?.SetTag("repository.operation", operationName);
        activity?.SetTag("repository.type", GetType().Name);

        // Allow caller to add custom tags
        enrichActivity?.Invoke(activity);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await operation();

            stopwatch.Stop();

            // Record success
            activity?.SetTag("repository.success", true);
            activity?.SetTag("repository.duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Ok);

            // Tag if result is null (useful for Get operations)
            if (result == null)
            {
                activity?.SetTag("repository.result", "null");
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Record failure
            activity?.SetTag("repository.success", false);
            activity?.SetTag("repository.duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetTag("repository.error.type", ex.GetType().Name);
            activity?.SetTag("repository.error.message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            throw;
        }
    }

    /// <summary>
    /// Executes a synchronous void repository operation with OpenTelemetry tracing.
    /// </summary>
    /// <param name="operationName">The name of the repository operation.</param>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="enrichActivity">Optional action to add custom tags to the activity.</param>
    protected async Task TraceAsync(
        string operationName,
        Func<Task> operation,
        Action<Activity?>? enrichActivity = null)
    {
        using var activity = ActivitySource.StartActivity($"Repository.{operationName}", ActivityKind.Internal);

        // Add common tags
        activity?.SetTag("repository.operation", operationName);
        activity?.SetTag("repository.type", GetType().Name);

        // Allow caller to add custom tags
        enrichActivity?.Invoke(activity);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await operation();

            stopwatch.Stop();

            // Record success
            activity?.SetTag("repository.success", true);
            activity?.SetTag("repository.duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Record failure
            activity?.SetTag("repository.success", false);
            activity?.SetTag("repository.duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetTag("repository.error.type", ex.GetType().Name);
            activity?.SetTag("repository.error.message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            throw;
        }
    }
}
