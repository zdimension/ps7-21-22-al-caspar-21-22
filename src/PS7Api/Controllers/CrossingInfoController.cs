using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using PS7Api.Models;
using PS7Api.Services;
using PS7Api.Utilities;

namespace PS7Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CrossingInfoController : ControllerBase
{
	private readonly Ps7Context _context;
	private readonly ILogger<CrossingInfoController> _logger;

	public CrossingInfoController(ILogger<CrossingInfoController> logger, Ps7Context context)
	{
		_logger = logger;
		_context = context;
	}

	/// <summary>
	///     Possibility to get a filtered list of crossing information
	/// </summary>
	/// <param name="validatedCrossing"></param>
	/// <param name="passengerCountMin"></param>
	/// <param name="passengerCountMax"></param>
	/// <param name="startDate"></param>
	/// <param name="endDate"></param>
	/// <param name="passengerType"></param>
	/// <param name="tollId"></param>
	/// <response code="200">A list of crossing information is returned</response>
	/// <response code="401">Unauthorized - route required authentication as CustomsOfficer</response>
	[AuthorizeRoles(UserRole.CustomsOfficer)]
	[HttpGet(Name = "GetCrossingInfoFilter")]
	[ProducesResponseType(typeof(List<CrossingInfo>), 200)]
	[ProducesResponseType(typeof(UnauthorizedResult), 401)]
	public async Task<IActionResult> GetCrossingInfoFilter(
			[FromQuery] bool validatedCrossing = true,
			[FromQuery] int? passengerCountMin = null,
			[FromQuery] int? passengerCountMax = null,
			[FromQuery] DateTime? startDate = null,
			[FromQuery] DateTime? endDate = null,
			[FromQuery] int? passengerType = null,
			[FromQuery] int? tollId = null
		)
	{
		var passengers = _context.CrossingInfos.AsQueryable().Where(p => p.EntryToll != null);

		if (validatedCrossing)
			passengers = passengers.Where(p => p.ExitTollId != null);

		if (startDate != null)
			passengers = passengers.Where(p => p.EntryTollTime >= startDate);

		if (endDate != null)
			passengers = passengers.Where(p => p.EntryTollTime <= endDate);

		if (passengerType != null)
			passengers = passengers.Where(p => p.TypeId == passengerType);

		if (tollId != null)
			passengers = passengers.Where(p =>
				p.EntryTollId == tollId || p.ExitTollId.HasValue && p.ExitTollId == tollId);

		if (passengerCountMin != null)
			passengers = passengers.Where(p => p.NbPassengers >= passengerCountMin);

		if (passengerCountMax != null)
			passengers = passengers.Where(p => p.NbPassengers <= passengerCountMax);

		return Ok(await passengers.ToListAsync());
	}

	/// <summary>
	///     Creates a new CrossingInfo based on the request body
	/// </summary>
	/// <param name="info">CrossingInfo to create</param>
	/// <response code="201">CrossingInfo created</response>
	/// <response code="401">Unauthorized - route required authentication as CustomsOfficer</response>
	[AuthorizeRoles(UserRole.CustomsOfficer)]
	[HttpPost(Name = "PostCrossingInfo")]
	[ProducesResponseType(typeof(CrossingInfo), 201)]
	[ProducesResponseType(typeof(UnauthorizedResult), 401)]
	public async Task<IActionResult> PostCrossingInfo(CrossingInfo info)
	{
		_context.CrossingInfos.Add(info);
		info.Person.CrossingInfos.Add(info);

		await _context.SaveChangesAsync();

		_logger.LogDebug("Posting crossing info");

		return CreatedAtAction("GetCrossingInfo", new { id = info.Id }, info);
	}
	
	[HttpPatch("{id}/EntryToll", Name = "EntryTollWithAsync")]
	public async Task<IActionResult> AddEntryToll([FromQuery] int id,
		[FromQuery] int tollId, [FromBody] DateTime? time = null)
	{
		var info = await _context.CrossingInfos
			.Include(c => c.Documents).ThenInclude(d => d.Anomalies)
			.FirstOrDefaultAsync(info => info.Id == id);
		if (info == null)
			return NotFound();

		if (info.Registered)
			return Conflict();

		info.EntryToll = await _context.TollOffices.FindAsync(tollId);
		info.EntryTollTime = time ?? DateTime.Now;

		var service = IOfficialValidationService.GetValidationService(new RegionInfo(info.EntryToll!.Country));
		foreach (var document in info.Documents)
		{
			switch (service.ValidateDocument(document))
			{
				case ValidationSuccess:
					document.Verified = true;
					break;
				case ValidationFailure result:
					document.Verified = false;
					document.Anomalies.AddRange(result.Errors.Select(msg => new DocumentAnomaly { Anomaly = msg }));
					break;
			}
		}

		await _context.SaveChangesAsync();
		return NoContent();
	}

	/// <summary>
	///     Scans a file and creates a document to add to the given CrossingInfo
	/// </summary>
	/// <param name="id">CrossingInfo</param>
	/// <param name="file"></param>
	/// <response code="201">Document created</response>
	/// <response code="401">Unauthorized - route required authentication as CustomsOfficer</response>
	/// <response code="404">CrossingInfo not found</response>
	[AuthorizeRoles(UserRole.CustomsOfficer)]
	[HttpPost("{id}/Document", Name = "ScanWithCrossingInfo")]
	[ProducesResponseType(typeof(CrossingInfo), 201)]
	[ProducesResponseType(typeof(UnauthorizedResult), 401)]
	[ProducesResponseType(typeof(NotFoundResult), 404)]
	public async Task<IActionResult> Scan(int id, IFormFile file)
	{
		var info = await _context.CrossingInfos.Include(info => info.EntryToll)
			.FirstOrDefaultAsync(info => info.Id == id);

		if (info == null)
			return NotFound();


		_logger.LogDebug("Scanning document size {Len}", file.Length);

		var ms = new MemoryStream();
		await file.CopyToAsync(ms);
		var document = new Document { Image = ms.ToArray() };

		if (info.Registered)
		{
			var service = IOfficialValidationService.GetValidationService(new RegionInfo(info.EntryToll!.Country));
			switch (service.ValidateDocument(document))
			{
				case ValidationSuccess:
					document.Verified = true;
					break;
				case ValidationFailure result:
					document.Verified = false;
					document.Anomalies.AddRange(result.Errors.Select(msg => new DocumentAnomaly { Anomaly = msg }));
					break;
			}
		}

		info.Documents.Add(document);

		await _context.SaveChangesAsync();

		return CreatedAtAction("GetCrossingInfo", new { id = info.Id }, info);
	}

	/// <summary>
	///     Gets the corresponding CrossingInfo
	/// </summary>
	/// <param name="id"></param>
	/// <response code="404">CrossingInfo not found</response>
	[HttpGet("{id}", Name = "GetCrossingInfo")]
	[ProducesResponseType(typeof(CrossingInfo), 200)]
	[ProducesResponseType(typeof(NotFoundResult), 404)]
	public async Task<IActionResult> GetCrossingInfo(int id)
	{
		var info = await _context.CrossingInfos.Include(c => c.Documents).ThenInclude(doc => doc.Anomalies)
			.FirstOrDefaultAsync(info => info.Id == id);

		if (info == null)
			return NotFound();

		return Ok(info);
	}

	/// <summary>
	///     Allows crossing
	/// </summary>
	/// <param name="id"></param>
	/// <param name="tollId"></param>
	/// <param name="time"></param>
	/// <response code="204">Crossing allowed</response>
	/// <response code="404">CrossingInfo not found</response>
	/// <response code="401">Unauthorized - route required authentication as CustomsOfficer</response>
	/// <response code="403">Crossing not allowed (documents might not all be valid or anomalies might have been reported)</response>
	/// <response code="409">Crossing not allowed (because performed previously)</response>
	/// <response code="422">Crossing not allowed (EntryToll must be provided)</response>
	[AuthorizeRoles(UserRole.CustomsOfficer)]
	[HttpPatch(Name = "AllowCrossing")]
	[ProducesResponseType(typeof(NoContentResult), 204)]
	[ProducesResponseType(typeof(NotFoundResult), 404)]
	[ProducesResponseType(typeof(UnauthorizedResult), 401)]
	[ProducesResponseType(typeof(ForbidResult), 403)]
	[ProducesResponseType(typeof(UnprocessableEntityResult), 422)]
	[ProducesResponseType(typeof(ConflictResult), 409)]
	public async Task<IActionResult> AllowCrossing(
		[FromQuery] int id,
		[FromQuery] int tollId,
		[FromBody] DateTime? time = null)
	{
		var info = await _context.CrossingInfos.Include(c => c.Documents).ThenInclude(d => d.Anomalies)
			.FirstOrDefaultAsync(info => info.Id == id);

		if (info == null)
			return NotFound();

		if (!info.Registered)
			return UnprocessableEntity();

		if (!info.AreAllDocumentsValid())
			return Forbid();

		if (info.Valid)
			return Conflict();

		info.Exit(tollId, time ?? DateTime.Now);

		await _context.SaveChangesAsync();

        return NoContent();
    }

}