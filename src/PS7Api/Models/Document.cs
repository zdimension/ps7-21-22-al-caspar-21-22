namespace PS7Api.Models;

public class Document
{
    public int Id { get; set; }
    public byte[] Image { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public bool Verified { get; set; } = false;
    public ICollection<DocumentAnomaly> Anomalies { get; set; }
}