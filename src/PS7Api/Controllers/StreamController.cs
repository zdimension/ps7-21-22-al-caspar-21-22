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
    
    // GET: api/Stream/filter?typePassenger=&period=&crossingPoints=&nbPassengers=4
    [HttpGet(Name = "GetStream")]
    public Task<IActionResult> Get([FromQuery] TypePassenger? typePassenger, string frequency, [FromQuery] Tuple<DateTime, DateTime>? period, [FromQuery] List<string>? crossingPoints, int nbPassengers = -1)
    {
        // Use LINQ to get list of passengers
        var passengers = from m in _context.StreamsFrontiers select m;

        if (nbPassengers >= 0)
        {
            passengers = passengers.Where(s => s.NbPassengers == nbPassengers);
        }

        if (typePassenger != null)
        {
            var passenger = typePassenger;
            passengers = passengers.Where(s => s.Type.Equals(passenger));
        }

        if (period != null)
        {
            passengers = passengers.Where(s => s.Period.Equals(period));
        }

        if (crossingPoints != null)
        {
            passengers = passengers.Where(s => s.CrossingPoints.Equals(crossingPoints));
        }

        return Task.FromResult<IActionResult>(Ok(passengers.ToList()));
    }
}