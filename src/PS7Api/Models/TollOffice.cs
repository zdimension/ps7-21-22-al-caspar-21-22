namespace PS7Api.Models;

public class TollOffice
{
    public TollOffice(string country)
    {
        Country = country;
    }

    public int Id { get; set; }
    public string Country { get; set; }
    
    
}