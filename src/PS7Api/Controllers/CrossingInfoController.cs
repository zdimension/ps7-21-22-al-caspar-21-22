using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PS7Api.Models;
using PS7Api.Utilities;

namespace PS7Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CrossingInfoController : ControllerBase
{
    private readonly Ps7Context _context;
    private readonly ILogger<CrossingInfoController> _logger;

    public CrossingInfoController(ILogger<CrossingInfoController> logger, Ps7Context context)
    {
        _logger = logger;
        _context = context;
    }
    
    // GET: api/CrossingInfo/...
    [AuthorizeRoles(UserRole.CustomsOfficer)]
    [HttpGet(Name = "GetCrossingInfoFilter")]
    [ProducesResponseType(typeof(List<CrossingInfo>), 200)]
    public async Task<IActionResult> GetCrossingInfoFilter(
        [FromQuery] int? passengerCountMin = null,
        [FromQuery] int? passengerCountMax = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? passengerType = null,
        [FromQuery] int? tollId = null
        )
    {
        var passengers = _context.CrossingInfos.AsQueryable();
        
        if (startDate != null)
        {
            passengers = passengers.Where(p => p.EntryTollTime >= startDate);
        }
        
        if (endDate != null)
        {
            passengers = passengers.Where(p => p.EntryTollTime <= endDate);
        }
        
        if (passengerType != null)
        {
            passengers = passengers.Where(p => p.TypeId == passengerType);
        }
        
        if (tollId != null)
        {
            passengers = passengers.Where(p => p.EntryTollId == tollId || (p.ExitTollId.HasValue && p.ExitTollId == tollId));
        }
        
        if (passengerCountMin != null)
        {
            passengers = passengers.Where(p => p.NbPassengers >= passengerCountMin);
        }
        
        if (passengerCountMax != null)
        {
            passengers = passengers.Where(p => p.NbPassengers <= passengerCountMax);
        }
        
        return Ok(await passengers.ToListAsync());
    }
    
    /// <summary>
    /// Creates a new CrossingInfo based on the request body
    /// </summary>
    /// <param name="info">CrossingInfo to create</param>
    /// <response code="201">CrossingInfo created</response>
    [AuthorizeRoles(UserRole.CustomsOfficer)]
    [HttpPost(Name = "PostCrossingInfo")]
    [ProducesResponseType(typeof(CrossingInfo), 201)]
    public async Task<IActionResult> PostCrossingInfo(CrossingInfo info)
    {
        _context.CrossingInfos.Add(info);
        
        await _context.SaveChangesAsync();
        
        _logger.LogDebug("Posting crossing info");
        
        return CreatedAtAction("GetCrossingInfo", new { id = info.Id }, info);
    }
    
    /// <summary>
    /// Scans a file and creates a document to add to the given CrossingInfo
    /// </summary>
    /// <param name="id">CrossingInfo</param>
    /// <param name="file"></param>
    /// <response code="201">Document created</response>
    /// <response code="404">CrossingInfo not found</response>
    [AuthorizeRoles(UserRole.CustomsOfficer)]
    [HttpPost("{id}/Document", Name = "ScanWithCrossingInfo")]
    [ProducesResponseType(typeof(CrossingInfo), 201)]
    [ProducesResponseType(typeof(NotFoundResult), 404)]
    public async Task<IActionResult> Scan(int id, IFormFile file)
    {
        
        var info = await _context.CrossingInfos.FirstOrDefaultAsync(info => info.Id == id);

        if (info == null)
            return NotFound();

        _logger.LogDebug("Scanning document size {Len}", file.Length);

        var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var document = new Document { Image = ms.ToArray() };

        //todo call a real service to check if the document is valid
        document.Verified = true;
        info.Documents.Add(document);
        
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetCrossingInfo", new { id = info.Id }, info);
    }
    
    /// <summary>
    /// Gets the corresponding CrossingInfo
    /// </summary>
    /// <param name="id"></param>
    /// <response code="404">CrossingInfo not found</response>
    [HttpGet("{id}", Name = "GetCrossingInfo")]
    [ProducesResponseType(typeof(CrossingInfo), 200)]
    [ProducesResponseType(typeof(NotFoundResult), 404)]
    public async Task<IActionResult> GetCrossingInfo(int id)
    {
        var info = await _context.CrossingInfos.Include(c => c.Documents).FirstOrDefaultAsync(info => info.Id == id);

        if (info == null)
            return NotFound();

        return Ok(info);
    }

    /// <summary>
    /// Allows crossing
    /// </summary>
    /// <param name="id"></param>
    /// <param name="tollId"></param>
    /// <param name="time"></param>
    /// <response code="204">Crossing allowed</response>
    /// <response code="404">CrossingInfo not found</response>
    /// <response code="403">Crossing not allowed (documents might not all be valid)</response>
    [AuthorizeRoles(UserRole.CustomsOfficer)]
    [HttpPatch(Name = "AllowCrossing")]
    [ProducesResponseType(typeof(NoContentResult), 204)]
    [ProducesResponseType(typeof(NotFoundResult), 404)]
    [ProducesResponseType(typeof(ForbidResult), 403)]
    public async Task<IActionResult> AllowCrossing(
        [FromQuery] int id,
        [FromQuery] int tollId,
        [FromBody] DateTime? time = null)
    {
        var info = await _context.CrossingInfos.Include(c => c.Documents).ThenInclude(d => d.Anomalies).FirstOrDefaultAsync(info => info.Id == id);
       
        if (info == null)
            return NotFound();
        
        if (!info.AreAllDocumentsValid())
            return Forbid();
        
        //todo if already allowed
        
        info.Exit(tollId, time ?? DateTime.Now);

        return NoContent();
    }
    
}