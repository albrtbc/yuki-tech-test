using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Yuki.Blog.Api.Contracts.v1.Requests;

namespace Yuki.Blog.Api.Filters;

/// <summary>
/// Schema filter that provides example values for request models in Swagger UI.
/// This sets the default values shown in the "Try it out" examples without hardcoding them in DTOs.
/// </summary>
public class ExampleValuesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Set example values for CreatePostRequest
        if (context.Type == typeof(CreatePostRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["authorId"] = new OpenApiString("00000000-0000-0000-0000-000000000001"),
                ["title"] = new OpenApiString("My Awesome Blog Post"),
                ["description"] = new OpenApiString("This is a brief description of my blog post."),
                ["content"] = new OpenApiString("This is the full content of my blog post with all the details...")
            };
        }
    }
}
