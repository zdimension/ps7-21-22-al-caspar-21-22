namespace PS7Api.Models;

public class Person
{
    public int Id { get; set; }
    public byte[]? Image { get; set; }
    public ICollection<CrossingInfo>? CrossingInfos { get; set; } = new List<CrossingInfo>();
}