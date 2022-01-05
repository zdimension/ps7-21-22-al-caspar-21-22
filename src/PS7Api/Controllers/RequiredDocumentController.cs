using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PS7Api.Models;

namespace PS7Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequiredDocumentController : ControllerBase
    {
        private readonly Ps7Context _context;
        private readonly ILogger<RequiredDocumentController> _logger;

        public RequiredDocumentController(ILogger<RequiredDocumentController> logger, Ps7Context context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: api/RequiredDocument/5
        [HttpGet()]
        public async Task<IActionResult> Get(string nationality, string origin, string destination)
        {
            Console.WriteLine("BONJOUR ON EST ENTRÉ DANS LA FONCTION");
            RegionInfo nat;
            RegionInfo from;
            RegionInfo to;
            try
            {
                nat = new RegionInfo(nationality);
                from = new RegionInfo(origin);
                to = new RegionInfo(destination);
            }
            catch (ArgumentException e)
            {
                return UnprocessableEntity();
            }

            var links = await _context.RequiredDocuments
                .Where(t => t.Country == nat.Name || t.Country == from.Name || t.Country == to.Name)
                .Distinct()
                .Include(t => t.Country)
                .SelectMany(t => t.Country)
                .ToListAsync();
            return Ok(links);
            //return Ok();
        }
    }
}
