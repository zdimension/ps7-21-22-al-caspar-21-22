using System.Text.Json.Serialization;

namespace PS7Api.Models;

public class Person
{
    public int Id { get; set; }
    public byte[]? Image { get; set; }
    
    [JsonIgnore]
    public ICollection<CrossingInfo> CrossingInfos { get; set; } = new List<CrossingInfo>();
}