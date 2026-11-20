
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

    private static readonly string[] ValidStatuses =
    {
        "Finished",
        "DNF",
        "DNS",
        "Disqualified"
    };

    private string? GetRole()
    {
        return HttpContext.Session.GetString("UserRole");
    }

    private int? GetUserId()
    {
        return HttpContext.Session.GetInt32("UserId");
    }

    private IQueryable<Result> ResultsWithDetails()
    {
        return _context.Results
            .AsNoTracking()
            .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Participant)
            .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Category)
                    .ThenInclude(c => c!.Event);
    }

    private static object ToResultResponse(Result r)
    {
        return new
        {
            r.ResultId,
            r.EnrolmentId,
            r.FinishTimeSeconds,
            r.OverallPosition,
            r.CategoryPosition,
            r.Status,
            r.RecordedAtUtc,

            ParticipantId = r.Enrolment?.ParticipantId,
            ParticipantEmail = r.Enrolment?.Participant?.Email,

            CategoryId = r.Enrolment?.CategoryId,
            CategoryName = r.Enrolment?.Category?.Name,

            EventId = r.Enrolment?.Category?.EventId,
            EventName = r.Enrolment?.Category?.Event?.Name
        };
    }

    // GET: api/Results
    // Organisers see their own results.
    // Participants see only their own results.
    [HttpGet]
    [SessionAuthorize]
    public async Task<IActionResult> GetResults()
    {
        var userId = GetUserId();
        var role = GetRole();

        if (userId == null)
            return Unauthorized();

        var query = ResultsWithDetails();

        if (role == "Organiser")
        {
            query = query.Where(r =>
                r.Enrolment!.Category!.Event!.OrganiserId == userId);
        }
        else if (role == "Participant")
        {
            query = query.Where(r =>
                r.Enrolment!.ParticipantId == userId);
        }
        else
        {
            return Forbid();
        }

        var results = await query
            .OrderBy(r => r.OverallPosition)
            .ToListAsync();

        return Ok(results.Select(ToResultResponse));
    }

    // GET: api/Results/1
    [HttpGet("{id:int}")]
    [SessionAuthorize]
    public async Task<IActionResult> GetResult(int id)
    {
        var userId = GetUserId();
        var role = GetRole();

        if (userId == null)
            return Unauthorized();

        var result = await ResultsWithDetails()
            .FirstOrDefaultAsync(r => r.ResultId == id);

        if (result == null)
            return NotFound(new { message = "Result was not found." });

        if (role == "Organiser" &&
            result.Enrolment?.Category?.Event?.OrganiserId != userId)
        {
            return Forbid();
        }

        if (role == "Participant" &&
            result.Enrolment?.ParticipantId != userId)
        {
            return Forbid();
        }

        if (role != "Organiser" && role != "Participant")
            return Forbid();

        return Ok(ToResultResponse(result));
    }

    // POST: api/Results
    [HttpPost]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> CreateResult(Result result)
    {
        var organiserId = GetUserId();

        if (organiserId == null)
            return Unauthorized();

        var enrolment = await _context.Enrolments
            .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentId == result.EnrolmentId);

        if (enrolment?.Category?.Event == null)
        {
            return NotFound(new
            {
                message = "Enrolment was not found."
            });
        }

        if (enrolment.Category.Event.OrganiserId != organiserId)
            return Forbid();

        if (enrolment.Status != "Confirmed")
        {
            return BadRequest(new
            {
                message = "Only confirmed enrolments can receive results."
            });
        }

        if (await _context.Results.AnyAsync(
            r => r.EnrolmentId == result.EnrolmentId))
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

        if (!ValidStatuses.Contains(result.Status))
        {
            return BadRequest(new
            {
                message = "Status must be Finished, DNF, DNS, or Disqualified."
            });
        }

        result.ResultId = 0;
        result.Status = result.Status.Trim();
        result.RecordedAtUtc = DateTime.UtcNow;
        result.Enrolment = null;

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        var created = await ResultsWithDetails()
            .FirstAsync(r => r.ResultId == result.ResultId);

        return CreatedAtAction(
            nameof(GetResult),
            new { id = result.ResultId },
            ToResultResponse(created));
    }

    // PUT: api/Results/1
    [HttpPut("{id:int}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> UpdateResult(
        int id,
        Result updatedResult)
    {
        var organiserId = GetUserId();

        if (organiserId == null)
            return Unauthorized();

        var result = await _context.Results
            .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Category)
                    .ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(r => r.ResultId == id);

        if (result == null)
            return NotFound(new { message = "Result was not found." });

        if (result.Enrolment?.Category?.Event?.OrganiserId != organiserId)
            return Forbid();

        if (updatedResult.FinishTimeSeconds < 0 ||
            updatedResult.OverallPosition < 1 ||
            updatedResult.CategoryPosition < 1)
        {
            return BadRequest(new
            {
                message = "Finish time and positions must be positive when provided."
            });
        }

        if (!ValidStatuses.Contains(updatedResult.Status))
        {
            return BadRequest(new
            {
                message = "Status must be Finished, DNF, DNS, or Disqualified."
            });
        }

        result.FinishTimeSeconds = updatedResult.FinishTimeSeconds;
        result.OverallPosition = updatedResult.OverallPosition;
        result.CategoryPosition = updatedResult.CategoryPosition;
        result.Status = updatedResult.Status.Trim();
        result.RecordedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Results/1
    [HttpDelete("{id:int}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> DeleteResult(int id)
    {
        var organiserId = GetUserId();

        if (organiserId == null)
            return Unauthorized();

        var result = await _context.Results
            .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Category)
                    .ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(r => r.ResultId == id);

        if (result == null)
            return NotFound(new { message = "Result was not found." });

        if (result.Enrolment?.Category?.Event?.OrganiserId != organiserId)
            return Forbid();

        _context.Results.Remove(result);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
