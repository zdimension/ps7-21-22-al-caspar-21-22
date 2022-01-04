namespace PS7Api.Models;

public class DocumentAnomaly
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public Document Document { get; set; }
    public AnomalyType Anomaly { get; set; }
}

public enum AnomalyType
{
    InvalidDocument,
    DifferentDocument
}