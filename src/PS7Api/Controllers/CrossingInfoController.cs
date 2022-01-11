using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic.CompilerServices;
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
    
    // POST: api/CrossingInfo/...
    [AuthorizeRoles(UserRole.CustomsOfficer)]
    [HttpPost(Name = "PostCrossingInfo")]
    public async Task<IActionResult> PostCrossingInfo(CrossingInfo info)
    {
        _context.CrossingInfos.Add(info);
        
        await _context.SaveChangesAsync();
        
        _logger.LogDebug("Posting crossing info");
        
        return CreatedAtAction("GetCrossingInfo", new { id = info.Id }, info);
    }
    
    [AuthorizeRoles(UserRole.CustomsOfficer)]
    [HttpPost("{id}", Name = "ScanWithCrossingInfo")]
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
    
    // GET: api/CrossingInfo/4
    [HttpGet("{id}", Name = "GetCrossingInfo")]
    public async Task<IActionResult> GetCrossingInfo(int id)
    {
        var info = await _context.CrossingInfos.Include(c => c.Documents).FirstOrDefaultAsync(info => info.Id == id);

        if (info == null)
            return NotFound();

        return Ok(info);
    }

    [AuthorizeRoles(UserRole.CustomsOfficer)]
    [HttpPatch(Name = "AllowCrossing")]
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
        
        info.Exit(tollId, time ?? DateTime.Now);

        return NoContent();
    }
    
}