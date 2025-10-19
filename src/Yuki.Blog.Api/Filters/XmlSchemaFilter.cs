using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Xml.Serialization;

namespace Yuki.Blog.Api.Filters;

/// <summary>
/// Schema filter to generate correct XML examples based on XmlElement attributes.
/// </summary>
public class XmlSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == null)
            return;

        // Check if the type has XmlRoot attribute
        var xmlRootAttribute = context.Type.GetCustomAttribute<XmlRootAttribute>();
        if (xmlRootAttribute != null && !string.IsNullOrEmpty(xmlRootAttribute.ElementName))
        {
            schema.Xml = new OpenApiXml
            {
                Name = xmlRootAttribute.ElementName
            };
        }

        // Process properties for XML element names
        if (schema.Properties != null && schema.Properties.Any())
        {
            var properties = context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var xmlElementAttribute = property.GetCustomAttribute<XmlElementAttribute>();
                if (xmlElementAttribute != null && !string.IsNullOrEmpty(xmlElementAttribute.ElementName))
                {
                    // Find the matching schema property (case-insensitive)
                    var schemaProperty = schema.Properties.FirstOrDefault(p =>
                        p.Key.Equals(property.Name, StringComparison.OrdinalIgnoreCase));

                    if (schemaProperty.Value != null)
                    {
                        // Set the XML element name to match the XmlElement attribute
                        schemaProperty.Value.Xml = new OpenApiXml
                        {
                            Name = xmlElementAttribute.ElementName
                        };
                    }
                }
            }
        }
    }
}
