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

    /// <summary>
    /// Gets the road traffic 
    /// </summary>
    /// <param name="from">Where the users come from</param>
    /// <param name="to">Where the users want to go</param>
    /// <param name="start">Gets the road traffic after given time. If it's not specified, it's set to Today</param>
    /// <param name="end">Gets the road traffic before given time. If it's not specified, it's set to Today. Needs the 'start' parameter</param>
    /// <response code="422">If the 'from' or 'to' parameters are empty, if 'end' is specified without 'start' or if 'end' is before 'start'</response>
    /// <response code="200">A list of CrossingInfo</response>
    [HttpGet(Name = "Pass")]
    [ProducesResponseType(typeof(List<CrossingInfo>), 200)]
    [ProducesResponseType(typeof(UnprocessableEntityResult), 422)]
    public async Task<IActionResult> Pass(
        [FromQuery] string from,
        [FromQuery] string to,
        [FromQuery] DateTime? start = null,
        [FromQuery] DateTime? end = null)
    {
        if (from.Length != 2 || to.Length != 2 || end != null && start == null || end < start)
            return UnprocessableEntity();
        var result = await _context.CrossingInfos.AsQueryable()
            .Where(info => info.Registered)
            .Where(info => info.Transport == Transport.Car || info.Transport == Transport.Truck)
            .Where(info => info.EntryToll!.Country == from)
            .Where(info => info.ExitToll != null && info.ExitToll.Country == to)
            .Where(info =>
                info.EntryTollTime!.Value.IsBetween(start ?? DateTime.Now.AddMinutes(-1), end ?? DateTime.Now)
                ||
                info.ExitTollTime != null &&
                info.ExitTollTime.Value.IsBetween(start ?? DateTime.Now.AddMinutes(-1), end ?? DateTime.Now))
            .Include(info => info.EntryToll)
            .Include(info => info.ExitToll)
            .ToListAsync();
        return Ok(result);
    }
}