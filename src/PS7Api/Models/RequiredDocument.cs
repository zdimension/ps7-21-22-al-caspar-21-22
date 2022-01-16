using System.ComponentModel.DataAnnotations;

namespace PS7Api.Models;

public class RequiredDocument
{
    [Key]
    public string Country { get; init; }

    public List<Link> Links { get; init; }
}

public class Link
{
    public Link(string url)
    {
        Url = url;
    }

    public int Id { get; set; }
    public string Url { get; init; }
}