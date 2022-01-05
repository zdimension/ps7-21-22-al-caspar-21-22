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

    // GET: api/DocumentAnomaly
    [AuthorizeRoles(UserRole.Administrator)]
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_context.DocumentAnomalies.AsAsyncEnumerable());
    }

    // GET: api/DocumentAnomaly/5
    [AuthorizeRoles(UserRole.Administrator)]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var docAno = await _context.DocumentAnomalies.FindAsync(id);

        if (docAno == null)
            return NotFound();

        return Ok(docAno);
    }

    // DELETE: api/DocumentAnomaly/5
    [AuthorizeRoles(UserRole.Administrator)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var docAno = await _context.DocumentAnomalies.FindAsync(id);

        if (docAno == null)
            return NotFound();

        _context.DocumentAnomalies.Remove(docAno);
        await _context.SaveChangesAsync();
        return Ok();
    }
}