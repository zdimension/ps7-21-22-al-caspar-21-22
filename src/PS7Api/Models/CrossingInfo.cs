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
    public DateTime? ExitTollTime { get; private set; } = null;
    public int EntryTollId { get; set; }
    public TollOffice? EntryToll { get; set; }
    public int? ExitTollId { get; private set; } = null;
    public TollOffice? ExitToll { get; private set; } = null;
    [NotMapped]
    public bool Valid => ExitTollId != null;

    public List<Document> Documents { get; init; } = new List<Document>();

    public bool AreAllDocumentsValid()
    {
        return Documents.Count > 0 && Documents.TrueForAll(d => d.Verified && d.Anomalies.Count == 0);    }

    public void Exit(TollOffice toll, DateTime time)
    {
        ExitToll = toll;
        ExitTollId = toll.Id;
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