using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Yuki.Blog.Api.Filters;

/// <summary>
/// Swagger operation filter to add X-API-Version header parameter to all endpoints
/// and clean up query parameters.
/// </summary>
public class ApiVersionHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        // Remove the computed "Includes" parameter if it exists
        var includesToRemove = operation.Parameters
            .Where(p => p.Name == "Includes" && p.In == ParameterLocation.Query)
            .ToList();

        foreach (var param in includesToRemove)
        {
            operation.Parameters.Remove(param);
        }

        // Check if X-API-Version parameter already exists (to avoid duplicates)
        var existingVersionHeader = operation.Parameters
            .FirstOrDefault(p => p.Name == "X-API-Version" && p.In == ParameterLocation.Header);

        if (existingVersionHeader != null)
        {
            // Update existing header with default value
            existingVersionHeader.Required = false;
            existingVersionHeader.Description = "API version (default: 1.0)";
            existingVersionHeader.Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new Microsoft.OpenApi.Any.OpenApiString("1.0")
            };
            existingVersionHeader.Example = new Microsoft.OpenApi.Any.OpenApiString("1.0");
        }
        else
        {
            // Add new X-API-Version header parameter
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-API-Version",
                In = ParameterLocation.Header,
                Description = "API version (default: 1.0)",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new Microsoft.OpenApi.Any.OpenApiString("1.0")
                },
                Example = new Microsoft.OpenApi.Any.OpenApiString("1.0")
            });
        }
    }
}
