using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Yuki.Blog.Api.Contracts.v1.Requests;

namespace Yuki.Blog.Api.Filters;

/// <summary>
/// Schema filter to customize query parameter types in Swagger documentation.
/// </summary>
public class QueryParametersSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // For PostQueryParameters, remove the Includes property as it's computed
        if (context.Type == typeof(PostQueryParameters))
        {
            // Remove the Includes property from schema if it exists
            if (schema.Properties?.ContainsKey("Includes") == true)
            {
                schema.Properties.Remove("Includes");
            }

            // Also try lowercase version
            if (schema.Properties?.ContainsKey("includes") == true)
            {
                schema.Properties.Remove("includes");
            }
        }
    }
}
