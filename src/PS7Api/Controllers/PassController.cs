using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PS7Api.Models;
using PS7Api.Utilities;

namespace PS7Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PassController : ControllerBase
{
    private readonly Ps7Context _context;
    private readonly ILogger<PassController> _logger;

    public PassController(Ps7Context context, ILogger<PassController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet(Name = "")]
    public async Task<IActionResult> Pass(
        [FromQuery] string from,
        [FromQuery] string to,
        [FromQuery] DateTime? start = null,
        [FromQuery] DateTime? end = null)
    {
        if (from.Length != 2 || to.Length != 2 || end != null && start == null || end < start)
            return UnprocessableEntity();
        var result = await _context.CrossingInfos.AsQueryable()
            .Where(info => info.Transport == Transport.Car || info.Transport == Transport.Truck)
            .Where(info => info.EntryToll.Country == from)
            .Where(info => info.ExitToll != null && info.ExitToll.Country == to)
            .Where(info =>
                info.EntryTollTime.IsBetween(start ?? DateTime.Now.AddMinutes(-1), end ?? DateTime.Now)
                ||
                info.ExitTollTime != null &&
                info.ExitTollTime.Value.IsBetween(start ?? DateTime.Now.AddMinutes(-1), end ?? DateTime.Now))
            .Include(info => info.EntryToll)
            .Include(info => info.ExitToll)
            .ToListAsync();
        return Ok(result);
    }
}