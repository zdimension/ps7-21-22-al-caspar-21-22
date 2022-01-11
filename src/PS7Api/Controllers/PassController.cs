using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PS7Api.Models;

namespace PS7Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PassController : ControllerBase
{
    
    private readonly Ps7Context _context;
    private readonly ILogger<DocumentController> _logger;

    public PassController(Ps7Context context, ILogger<DocumentController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet(Name = "")]
    public async Task<IActionResult> Pass([FromQuery(Name = "from")] string from, [FromQuery(Name = "to")] string to,
        [FromQuery(Name = "start")] DateTime? start = null, [FromQuery(Name = "end")] DateTime? end = null)
    {
        if (from.Length != 2 || to.Length != 2 || (end != null && start == null) || (end < start))
            return UnprocessableEntity();
        var result = await _context.CrossingInfos.AsQueryable()
            .Where(info => info.Transport == Transport.Car || info.Transport == Transport.Truck)
            .Where(info => info.EntryToll.Country == from)
            .Where(info => info.ExitToll != null && info.ExitToll.Country == to)
            .Where(info =>
                (info.EntryTollTime >= (start ?? DateTime.Now.AddMinutes(-1)) &&
                 info.EntryTollTime <= (end ?? DateTime.Now))
                ||
                (info.ExitTollTime != null && info.ExitTollTime >= (start ?? DateTime.Now.AddMinutes(-1)) &&
                 info.ExitTollTime <= (end ?? DateTime.Now)))
            .Include(info => info.EntryToll)
            .Include(info => info.ExitToll)
            .ToListAsync();
        return Ok(result);
    }
    
}