namespace PS7Api.Models;

public class RequiredDocument
{
	public string Country { get; }
	public List<string> Links { get; }

	public RequiredDocument(string country, List<string> links)
	{
		Country = country;
		Links = links;
	}
}