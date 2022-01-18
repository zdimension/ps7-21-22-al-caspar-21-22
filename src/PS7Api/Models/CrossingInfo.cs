using System.ComponentModel.DataAnnotations.Schema;

namespace PS7Api.Models;

public class CrossingInfo
{
    public CrossingInfo(TollOffice entryToll)
    {
        EntryToll = entryToll;
    }

    public CrossingInfo()
    {
    }

    public int Id { get; set; }
    public int NbPassengers { get; set; }
    public int TypeId { get; set; }
    public TypePassenger? Type { get; set; }
    public DateTime? EntryTollTime { get; set; }
    public DateTime? ExitTollTime { get; set; }
    public int? EntryTollId { get; set; }
    public TollOffice? EntryToll { get; set; }
    public int? ExitTollId { get; set; }
    public TollOffice? ExitToll { get; set; }

    [NotMapped]
    public bool Valid => ExitTollId != null;
    [NotMapped]
    public bool Registered => EntryTollId != null;
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public Transport Transport { get; set; }
    
    public Person? Person { get; set; }

    public bool AreAllDocumentsValid()
    {
        return Documents.Any() && Documents.All(d => d.Verified && !d.Anomalies.Any());
    }

    public void Exit(int tollId, DateTime time)
    {
        ExitTollId = tollId;
        ExitTollTime = time;
    }
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
    Boat,
    Ship,
    Airplace,
    Car,
    Train,
    Truck
}