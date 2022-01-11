using System.ComponentModel.DataAnnotations.Schema;
using PS7Api.Controllers;

namespace PS7Api.Models;

public class CrossingInfo
{
    public int Id { get; set; }
    public int NbPassengers { get; set; }
    public int TypeId { get; set; }
    public TypePassenger? Type { get; set; }
    public DateTime EntryTollTime { get; set; }
    public DateTime? ExitTollTime { get; set; } = null;
    public int EntryTollId { get; set; }
    public TollOffice? EntryToll { get; set; }
    public int? ExitTollId { get; set; } = null;
    public TollOffice? ExitToll { get; set; } = null;
    [NotMapped]
    public bool Valid => ExitTollId != null;
    
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    
    public bool AreAllDocumentsValid()
    {
        return Documents.Count > 0 && Documents.All(d => d.Verified && d.Anomalies.Count == 0);
    }

    public void Exit(int tollId, DateTime time)
    {
        ExitTollId = tollId;
        ExitTollTime = time;
    }
    public Transport Transport { get; set; }
}

public abstract class TypePassenger
{
    public int Id { get; set; }
}

public class Human : TypePassenger
{
    public HumanEnum Type { get; set; }
}

public enum HumanEnum
{
    Tourist,
    Professional
}

public class Merchendise : TypePassenger
{
    public string TypeVehicle { get; set; }
    public string TypeMerchendise { get; set; }
    public string QuantityMerchendise { get; set; }
}

public enum Transport
{
    Boat, Ship, Airplace, Car, Train, Truck
}