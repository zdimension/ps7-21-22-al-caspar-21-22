using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PS7Api.Models;

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
    /*[AuthorizeRoles(UserRole.CustomsOfficer)]
    [HttpPost(Name = "Scan")]
    public async Task<IActionResult> Scan(IFormFile file)
    {
        _logger.LogDebug("Scanning document size {Len}", file.Length);

        var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var document = new Document { Image = ms.ToArray() };
        
        _context.Documents.Add(document);
        
        //todo call a real service to check if the document is valid
       
        await _context.SaveChangesAsync();

        return CreatedAtAction("Get", new { id = document.Id }, document);
    }*/

    /// <summary>
    ///     Gets a document
    /// </summary>
    /// <param name="id"></param>
    /// <response code="200">Returns the document</response>
    /// <response code="404">Document not found</response>
    [HttpGet("{id}", Name = "Get")]
    [ProducesResponseType(typeof(Document), 200)]
    [ProducesResponseType(typeof(NotFoundResult), 404)]
    public async Task<IActionResult> Get(int id)
    {
        var doc = await _context.Documents.Include(doc => doc.Anomalies).FirstOrDefaultAsync(doc => doc.Id == id);

        if (doc == null)
            return NotFound();

        return Ok(doc);
    }

    /// <summary>
    ///     Gets the image of the document
    /// </summary>
    /// <param name="id"></param>
    /// <response code="200">Returns the image of the document</response>
    /// <response code="404">Document not found</response>
    [HttpGet("{id}/Image", Name = "Image")]
    [ProducesResponseType(typeof(File), 200)]
    [ProducesResponseType(typeof(NotFoundResult), 404)]
    public async Task<IActionResult> Image(int id)
    {
        var doc = await _context.Documents.FindAsync(id);

        if (doc == null)
            return NotFound();

        return File(doc.Image, "image/*");
    }

    /// <summary>
    ///     Delete document
    /// </summary>
    /// <param name="id"></param>
    /// <response code="204">Document deleted</response>
    /// <response code="404">Document not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(NoContentResult), 204)]
    [ProducesResponseType(typeof(NotFoundResult), 404)]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _context.Documents.FindAsync(id);

        if (doc == null)
            return NotFound();

        _context.Documents.Remove(doc);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    ///     Patch document
    /// </summary>
    /// <param name="id"></param>
    /// <param name="patchDoc"></param>
    /// <response code="204">Invalid parameters given</response>
    /// <response code="422">Invalid parameters given</response>
    /// <response code="404">Document not found</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(NoContentResult), 204)]
    [ProducesResponseType(typeof(UnprocessableEntityResult), 422)]
    [ProducesResponseType(typeof(NotFoundResult), 404)]
    public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<Document>? patchDoc)
    {
        if (patchDoc == null)
            return UnprocessableEntity();

        var documentFromDb = await _context.Documents.FindAsync(id);

        if (documentFromDb == null)
            return NotFound();

        patchDoc.ApplyTo(documentFromDb);

        var isValid = TryValidateModel(documentFromDb);

        if (!isValid)
            return UnprocessableEntity(ModelState);

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
        return Ok(); //todo find a more relevant response
    }

    public record AnomaliesBody(string[] Anomalies);
}