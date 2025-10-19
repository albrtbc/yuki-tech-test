using System.Xml.Serialization;

namespace Yuki.Blog.Api.Contracts.v1.Responses;

/// <summary>
/// Health check response model.
/// </summary>
[XmlRoot("HealthCheckResponse")]
public class HealthCheckResponse
{
    /// <summary>
    /// Overall health status.
    /// </summary>
    [XmlElement("Status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Total duration of all health checks in milliseconds.
    /// </summary>
    [XmlElement("TotalDuration")]
    public double TotalDuration { get; set; }

    /// <summary>
    /// Individual health check results.
    /// </summary>
    [XmlArray("Checks")]
    [XmlArrayItem("Check")]
    public List<HealthCheckEntry> Checks { get; set; } = new();
}

/// <summary>
/// Individual health check entry.
/// </summary>
[XmlRoot("HealthCheckEntry")]
public class HealthCheckEntry
{
    /// <summary>
    /// Name of the health check.
    /// </summary>
    [XmlElement("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Status of the health check.
    /// </summary>
    [XmlElement("Status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Duration of the health check in milliseconds.
    /// </summary>
    [XmlElement("Duration")]
    public double Duration { get; set; }

    /// <summary>
    /// Optional description of the health check.
    /// </summary>
    [XmlElement("Description", IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// Exception message if the health check failed.
    /// </summary>
    [XmlElement("Exception", IsNullable = true)]
    public string? Exception { get; set; }

    /// <summary>
    /// Additional data from the health check.
    /// </summary>
    [XmlIgnore]
    public IDictionary<string, object>? Data { get; set; }
}
