using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PS7Api.Models;

public class DocumentAnomaly
{
    [Key]
    public int Id { get; set; }

    public int DocumentId { get; set; }

    [JsonIgnore]
    public Document Document { get; set; }

    public string Anomaly { get; set; }
}