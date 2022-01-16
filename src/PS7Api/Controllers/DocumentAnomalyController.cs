using Microsoft.AspNetCore.Mvc;
using PS7Api.Models;
using PS7Api.Utilities;

namespace PS7Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DocumentAnomalyController : ControllerBase
{
    private readonly Ps7Context _context;
    private readonly ILogger<DocumentAnomalyController> _logger;

    public DocumentAnomalyController(ILogger<DocumentAnomalyController> logger, Ps7Context context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    ///     Get all document anomalies
    /// </summary>
    /// <response code="200">Returns the list of document anomalies</response>
    /// <response code="401">Unauthorized - route required authentication as Administrator</response>
    [AuthorizeRoles(UserRole.Administrator)]
    [HttpGet]
    [ProducesResponseType(typeof(IAsyncEnumerable<DocumentAnomaly>), 200)]
    [ProducesResponseType(typeof(UnauthorizedResult), 401)]
    public IActionResult Get()
    {
        return Ok(_context.DocumentAnomalies.AsAsyncEnumerable());
    }

    /// <summary>
    ///     Gets an anomaly by id
    /// </summary>
    /// <param name="id"></param>
    /// <response code="200">Returns the anomaly</response>
    /// <response code="401">Unauthorized - route required authentication as Administrator</response>
    /// <response code="404">Anomaly not found</response>
    [AuthorizeRoles(UserRole.Administrator)]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DocumentAnomaly), 200)]
    [ProducesResponseType(typeof(UnauthorizedResult), 401)]
    [ProducesResponseType(typeof(NotFoundResult), 404)]
    public async Task<IActionResult> Get(int id)
    {
        var docAno = await _context.DocumentAnomalies.FindAsync(id);

        if (docAno == null)
            return NotFound();

        return Ok(docAno);
    }

    /// <summary>
    ///     Deletes an anomaly by id
    /// </summary>
    /// <param name="id"></param>
    /// <response code="204">Anomaly deleted</response>
    /// <response code="401">Unauthorized - route required authentication as Administrator</response>
    /// <response code="404">Anomaly not found</response>
    [AuthorizeRoles(UserRole.Administrator)]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(NoContentResult), 204)]
    [ProducesResponseType(typeof(UnauthorizedResult), 401)]
    [ProducesResponseType(typeof(NotFoundResult), 404)]
    public async Task<IActionResult> Delete(int id)
    {
        var docAno = await _context.DocumentAnomalies.FindAsync(id);

        if (docAno == null)
            return NotFound();

        _context.DocumentAnomalies.Remove(docAno);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}