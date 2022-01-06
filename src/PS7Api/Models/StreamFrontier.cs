namespace PS7Api.Models;

public class StreamFrontier
{
    public int NbPassengers { get; set; }
    public TypePassenger Type { get; set; }
    public string Frequency { get; set; }
    public System.Tuple<DateTime, DateTime> Period { get; set; }
    public List<string> CrossingPoints { get; set; }
}

public class TypePassenger
{
    
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