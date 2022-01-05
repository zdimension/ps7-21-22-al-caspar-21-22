using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PS7Api.Models;
using PS7Api.Utilities;

namespace PS7Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DocumentController : ControllerBase
{
    private readonly Ps7Context _context;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(ILogger<DocumentController> logger, Ps7Context context)
    {
        _logger = logger;
        _context = context;
    }

    // POST: api/Document
    [AuthorizeRoles(UserRole.CustomsOfficer)]
    [HttpPost(Name = "Scan")]
    public async Task<IActionResult> Scan(IFormFile file)
    {
        _logger.LogDebug("Scanning document size {Len}", file.Length);

        var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var document = new Document { Image = ms.ToArray() };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return CreatedAtAction("Get", new { id = document.Id }, document);
    }

    // GET: api/Document/5
    [HttpGet("{id}", Name = "Get")]
    public async Task<IActionResult> Get(int id)
    {
        var doc = await _context.Documents.Include(doc => doc.Anomalies).FirstOrDefaultAsync(doc => doc.Id == id);

        if (doc == null)
            return NotFound();

        return Ok(doc);
    }

    // GET: api/Document/5/Image
    [HttpGet("{id}/Image", Name = "Image")]
    public async Task<IActionResult> Image(int id)
    {
        var doc = await _context.Documents.FindAsync(id);

        if (doc == null)
            return NotFound();

        return File(doc.Image, "image/*");
    }

    // DELETE: api/Document/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _context.Documents.FindAsync(id);

        if (doc == null)
            return NotFound();

        _context.Documents.Remove(doc);
        await _context.SaveChangesAsync();
        return Ok();
    }
    
    // PATCH: api/Document/4
    [HttpPatch("{id}")]
    public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<Document>? patchDoc)
    {
        if (patchDoc == null)
        {
            return BadRequest();
        }
 
        var documentFromDb = await _context.Documents.FindAsync(id);
 
        if (documentFromDb == null)
        {
            return NotFound();
        }
 
        patchDoc.ApplyTo(documentFromDb);
 
        var isValid = TryValidateModel(documentFromDb);
 
        if (!isValid){
            return BadRequest(ModelState);
        }
 
        await _context.SaveChangesAsync();
 
        return NoContent();
    }

    // POST: api/Document/5/Non-compliant
    [HttpPost("{id}/Non-compliant")]
    public async Task<IActionResult> NonCompliant(int id, [FromBody] AnomaliesBody anomalies)
    {
        var doc = await _context.Documents.FindAsync(id);
        if (doc == null)
            return NotFound();
        if (anomalies.Anomalies.Length == 0)
            return UnprocessableEntity();
        foreach (var anomaly in anomalies.Anomalies)
            doc.Anomalies.Add(new DocumentAnomaly { DocumentId = id, Document = doc, Anomaly = anomaly });
        _logger.LogInformation("{}", doc.Anomalies);
        await _context.SaveChangesAsync();
        return Ok();
    }
    
    public record AnomaliesBody(string[] Anomalies);
}