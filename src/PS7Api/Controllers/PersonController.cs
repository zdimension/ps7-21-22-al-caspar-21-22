using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PS7Api.Models;
using PS7Api.Services;

namespace PS7Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PersonController : ControllerBase
{
    
    private readonly Ps7Context _context;
    private readonly ILogger<PersonController> _logger;
    private readonly IFaceMatchService _faceMatch;

    public PersonController(ILogger<PersonController> logger, Ps7Context context, IFaceMatchService faceMatch)
    {
        _logger = logger;
        _context = context;
        _faceMatch = faceMatch;
    }

    [HttpPost(Name = "Create")]
    public async Task<IActionResult> Create(IFormFile photoFile)
    {
        var memoryStream = new MemoryStream();
        await photoFile.CopyToAsync(memoryStream);
        var photo = memoryStream.ToArray();
        var person = new Person() {Image = photo};
        
        _context.Persons.Add(person);

        await _context.SaveChangesAsync();

        _logger.LogDebug("Posting person");

        return CreatedAtAction("GetPhoto", new { id = person.Id }, person);
    }

    /// <summary>
    /// Gets the person id associated with the photo
    /// </summary>
    /// <param name="photoFile">An image of the person</param>
    /// <response code="200">The id of the person</response>
    /// <response code="404">If no association has been found</response>
    [HttpPost("GetPhoto", Name = "GetPhoto")]
    [ProducesResponseType(typeof(NotFoundResult), 404)]
    [ProducesResponseType(typeof(OkResult), 200)]
    public async Task<IActionResult> GetPhoto(IFormFile photoFile)
    {
        var memoryStream = new MemoryStream();
        await photoFile.CopyToAsync(memoryStream);
        var photo = memoryStream.ToArray();
        var person = (await _context.Persons.ToListAsync())
            .Select(p => new { p, Score = _faceMatch.GetMatchScore(p.Image!, photo)})
            .OrderByDescending(t => t.Score)
            .FirstOrDefault()?.p;
        if (person != null)
            return Ok(person.Id);
        return NotFound();
    }
    
    /// <summary>
    /// Gets the CrossingInfos of a person
    /// </summary>
    /// <param name="id">The id of the person</param>
    /// <response code="200">A list of CrossingInfo</response>
    /// <response code="404">If no person has the given id</response>
    [HttpGet("{id:int}", Name = "GetCrossingInfos")]
    [ProducesResponseType(typeof(NotFoundResult), 404)]
    [ProducesResponseType(typeof(List<CrossingInfo>), 200)]
    public async Task<IActionResult> GetCrossingInfos(int id)
    {
        var person = await _context.Persons.FindAsync(id);
        if(person == null)
            return NotFound();
        return Ok(person.CrossingInfos);
    }
    
}