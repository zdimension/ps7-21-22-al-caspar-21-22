using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic.CompilerServices;
using PS7Api.Models;

namespace PS7Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StreamController : ControllerBase
{
    private readonly Ps7Context _context;
    private readonly ILogger<StreamController> _logger;

    public StreamController(ILogger<StreamController> logger, Ps7Context context)
    {
        _logger = logger;
        _context = context;
    }

    // ReSharper disable twice InconsistentNaming
    public record CountRange(int? passengerCountMin = null, int? passengerCountMax = null);
    
    // GET: api/Stream/filter?typePassenger=&period=&crossingPoints=&nbPassengers=4
    [HttpGet(Name = "GetStream")]
    public async Task<IActionResult> Get(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? passengerType = null,
        [FromQuery] int? tollId = null,
        [FromQuery] CountRange? passengerCount = null)
    {
        var passengers = _context.StreamsFrontiers.AsQueryable();
        
        if (startDate != null)
        {
            passengers = passengers.Where(p => p.WaitEnd >= startDate);
        }
        
        if (endDate != null)
        {
            passengers = passengers.Where(p => p.WaitEnd <= endDate);
        }
        
        if (passengerType != null)
        {
            passengers = passengers.Where(p => p.TypeId == passengerType);
        }
        
        if (tollId != null)
        {
            passengers = passengers.Where(p => p.EntryTollId == tollId || p.ExitTollId == tollId);
        }
        
        if (passengerCount != null)
        {
            passengers = passengers.Where(p => p.NbPassengers >= passengerCount.passengerCountMin && p.NbPassengers <= passengerCount.passengerCountMax);
        }
        
        return Ok(await passengers.ToListAsync());
    }
}