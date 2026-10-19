
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Authorization;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ResultsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // GET: api/Results
    [SessionAuthorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Result>>> GetResults()
    {
        var results = await _context.Results
            .AsNoTracking()
            .ToListAsync();

        return Ok(results);
    }

    // GET: api/Results/1
    [SessionAuthorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result>> GetResult(int id)
    {
        var result = await _context.Results
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ResultId == id);

        if (result == null)
        {
            return NotFound(new
            {
                message = $"Result with ID {id} was not found."
            });
        }

        return Ok(result);
    }

    // POST: api/Results
    [HttpPost]
    [SessionAuthorize("Organiser")]
    public async Task<ActionResult<Result>> CreateResult(Result result)
    {
        var organiserId = HttpContext.Session.GetInt32("UserId");

        if (organiserId == null)
            return Unauthorized();

        var enrolment = await _context.Enrolments
            .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentId == result.EnrolmentId);

        if (enrolment?.Category?.Event == null)
        {
            return NotFound(new { message = "Enrolment was not found." });
        }

        if (enrolment.Category.Event.OrganiserId != organiserId.Value)
        {
            return Forbid();
        }

        var resultExists = await _context.Results
            .AnyAsync(r => r.EnrolmentId == result.EnrolmentId);

        if (resultExists)
        {
            return Conflict(new
            {
                message = "A result already exists for this enrolment."
            });
        }

        if (result.FinishTimeSeconds < 0 ||
            result.OverallPosition < 1 ||
            result.CategoryPosition < 1)
        {
            return BadRequest(new
            {
                message = "Finish time and positions must be positive when provided."
            });
        }

        result.ResultId = 0;
        result.RecordedAtUtc = DateTime.UtcNow;

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetResult),
            new { id = result.ResultId },
            result);
    }

    // PUT: api/Results/1
    [HttpPut("{id:int}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> UpdateResult(int id, Result updatedResult)
    {
        var organiserId = HttpContext.Session.GetInt32("UserId");

        if (organiserId == null)
            return Unauthorized();

        var result = await _context.Results
            .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Category)
                    .ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(r => r.ResultId == id);

        if (result == null)
        {
            return NotFound(new { message = "Result was not found." });
        }

        if (result.Enrolment?.Category?.Event?.OrganiserId != organiserId.Value)
        {
            return StatusCode(403, new
            {
                message = "You can only update results for your own events."
            });
        }

        if (updatedResult.FinishTimeSeconds < 0 ||
            updatedResult.OverallPosition < 1 ||
            updatedResult.CategoryPosition < 1)
        {
            return BadRequest(new
            {
                message = "Finish time and positions must be positive when provided."
            });
        }

        result.FinishTimeSeconds = updatedResult.FinishTimeSeconds;
        result.OverallPosition = updatedResult.OverallPosition;
        result.CategoryPosition = updatedResult.CategoryPosition;
        result.Status = updatedResult.Status;
        result.RecordedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Results/1
    [HttpDelete("{id:int}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> DeleteResult(int id)
    {
        var organiserId = HttpContext.Session.GetInt32("UserId");

        if (organiserId == null)
            return Unauthorized();

        var result = await _context.Results
            .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Category)
                    .ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(r => r.ResultId == id);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Result was not found."
            });
        }

        if (result.Enrolment?.Category?.Event?.OrganiserId != organiserId.Value)
        {
            return StatusCode(403, new
            {
                message = "You can only delete results for your own events."
            });
        }

        _context.Results.Remove(result);
        await _context.SaveChangesAsync();

        return NoContent();
    }

}
