using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PS7Api.Models;

namespace PS7Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RequiredDocumentController : ControllerBase
{
    private readonly Ps7Context _context;
    private readonly ILogger<RequiredDocumentController> _logger;

    public RequiredDocumentController(ILogger<RequiredDocumentController> logger, Ps7Context context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    ///     Retrieves the links the the websites listing the required documents
    /// </summary>
    /// <param name="nationality">Given in the two-letter code defined in ISO 3166 for the country/region format</param>
    /// <param name="origin">Given in the two-letter code defined in ISO 3166 for the country/region format</param>
    /// <param name="destination">Given in the two-letter code defined in ISO 3166 for the country/region format</param>
    /// <response code="200">Returns the list of links</response>
    /// <response code="422">Invalid parameters given</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<string>), 200)]
    [ProducesResponseType(typeof(UnprocessableEntityResult), 422)]
    public async Task<IActionResult> Get(string nationality, string origin, string destination)
    {
        RegionInfo nat;
        RegionInfo from;
        RegionInfo to;
        try
        {
            nat = new RegionInfo(nationality);
            from = new RegionInfo(origin);
            to = new RegionInfo(destination);
        }
        catch (ArgumentException e)
        {
            return UnprocessableEntity();
        }

        var links = await _context.RequiredDocuments
            .Where(t => t.Country == nat.Name || t.Country == from.Name || t.Country == to.Name)
            .Distinct()
            .Include(t => t.Links)
            .SelectMany(t => t.Links)
            .Select(l => l.Url)
            .ToListAsync();
        return Ok(links);
    }
}