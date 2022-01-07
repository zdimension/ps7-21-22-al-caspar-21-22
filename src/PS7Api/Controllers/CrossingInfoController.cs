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

    // ReSharper disable twice InconsistentNaming
    public record CountRange(int? passengerCountMin = null, int? passengerCountMax = null);
    
    // GET: api/CrossingInfo/...
    [AuthorizeRoles(UserRole.CustomsOfficer)]
    [HttpGet(Name = "GetCrossingInfoFilter")]
    public async Task<IActionResult> GetCrossingInfoFilter(
        [FromQuery] CountRange passengerCount,
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
        
        if (passengerCount.passengerCountMin != null)
        {
            passengers = passengers.Where(p => p.NbPassengers >= passengerCount.passengerCountMin);
        }
        
        if (passengerCount.passengerCountMax != null)
        {
            passengers = passengers.Where(p => p.NbPassengers <= passengerCount.passengerCountMax);
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
    
    // GET: api/CrossingInfo/4
    [HttpGet("{id}", Name = "GetCrossingInfo")]
    public async Task<IActionResult> GetCrossingInfo(int id)
    {
        var info = await _context.CrossingInfos.Include(info => info.Id).FirstOrDefaultAsync(info => info.Id == id);

        if (info == null)
            return NotFound();

        return Ok(info);
    }
}