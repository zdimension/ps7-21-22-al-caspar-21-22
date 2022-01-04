using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PS7Api.Models;

namespace PS7Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly ILogger<DocumentController> _logger;
        private readonly Ps7Context _context;

        public DocumentController(ILogger<DocumentController> logger, Ps7Context context)
        {
            _logger = logger;
            _context = context;
        }
        
        // POST: api/Document
        [Authorize(Roles = UserRoles.CustomsOfficer)]
        [HttpPost(Name = "Scan")]
        public async Task<IActionResult> Scan(IFormFile file)
        {
            _logger.LogDebug("Scanning document size {Len}", file.Length);
            
            var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var document = new Document() { Image = ms.ToArray() };
            
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { id = document.Id }, document);
        }

        // GET: api/Document/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<IActionResult> Get(int id)
        {
            var doc = await _context.Documents.FindAsync(id);
                
            if (doc == null)
            {
                return NotFound();
            }
                
            return Ok(doc);
        }

        // DELETE: api/Document/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _context.Documents.FindAsync(id);
            
            if (doc == null)
            {
                return NotFound();
            }
            
            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
