
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Authorization;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using System.ComponentModel.DataAnnotations;
using RaceDay.Api.DTOs;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[SessionAuthorize("Participant")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EnrolmentsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // GET: api/Enrolments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Enrolment>>> GetEnrolments()
    {
        var participantId = HttpContext.Session.GetInt32("UserId");

        if (participantId == null)
            return Unauthorized();

        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Where(e => e.ParticipantId == participantId.Value)
            .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
            .Include(e => e.Result)
            .ToListAsync();

        return Ok(enrolments);
    }

    // GET: api/Enrolments/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Enrolment>> GetEnrolment(int id)
    {
        var participantId = HttpContext.Session.GetInt32("UserId");

        if (participantId == null)
            return Unauthorized();

        var enrolment = await _context.Enrolments
            .AsNoTracking()
            .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
            .Include(e => e.Result)
            .FirstOrDefaultAsync(e =>
                e.EnrolmentId == id &&
                e.ParticipantId == participantId.Value);

        if (enrolment == null)
        {
            return NotFound(new
            {
                message = "Enrolment was not found."
            });
        }

        return Ok(enrolment);
    }

    // GET: api/Enrolments/my
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<Enrolment>>> GetMyEnrolments()
    {
        return await GetEnrolments();
    }

    // POST: api/Enrolments
    [HttpPost]
    public async Task<ActionResult<Enrolment>> CreateEnrolment(
        CreateEnrolmentRequest request)
    {
        var participantId = HttpContext.Session.GetInt32("UserId");

        if (participantId == null)
            return Unauthorized();

        var category = await _context.EventCategories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Event category was not found."
            });
        }

        if (category.Event == null ||
            category.Event.Status != "Published" ||
            category.Event.EventDate <= DateTime.UtcNow ||
            category.Event.RegistrationCloseDate <= DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message = "This event is not currently open for enrolment."
            });
        }


        var enrolledCount = await _context.Enrolments.CountAsync(e =>
    e.CategoryId == request.CategoryId &&
    e.Status != "Cancelled");

        if (enrolledCount >= category.Capacity)
        {
            return Conflict(new
            {
                message = "This event category is full."
            });
        }


        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == participantId.Value);

        if (profile?.DateOfBirth == null)
        {
            return BadRequest(new
            {
                message = "Please add your date of birth to your profile before enrolling."
            });
        }

        var today = DateTime.UtcNow.Date;
        var dateOfBirth = profile.DateOfBirth.Value.Date;

        var age = today.Year - dateOfBirth.Year;

        if (dateOfBirth > today.AddYears(-age))
        {
            age--;
        }

        if (age < category.MinimumAge)
        {
            return BadRequest(new
            {
                message = $"You must be at least {category.MinimumAge} years old to enrol in this category."
            });
        }

        var alreadyEnrolled = await _context.Enrolments.AnyAsync(e =>
            e.CategoryId == request.CategoryId &&
            e.ParticipantId == participantId.Value);

        if (alreadyEnrolled)
        {
            return Conflict(new
            {
                message = "You are already enrolled in this category."
            });
        }

        var enrolment = new Enrolment
        {
            CategoryId = request.CategoryId,
            ParticipantId = participantId.Value,
            EmergencyContactName = request.EmergencyContactName.Trim(),
            EmergencyContactPhone = request.EmergencyContactPhone.Trim(),
            EnrolledAtUtc = DateTime.UtcNow,
            Status = "Pending"
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEnrolment),
            new { id = enrolment.EnrolmentId },
            enrolment);
    }

    // GET: api/Enrolments/organiser
    [HttpGet("organiser")]
    [SessionAuthorize("Organiser")]
    public async Task<ActionResult<IEnumerable<Enrolment>>> GetOrganiserEnrolments()
    {
        var organiserId = HttpContext.Session.GetInt32("UserId");

        if (organiserId == null)
            return Unauthorized();

        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Where(e => e.Category != null &&
                        e.Category.Event != null &&
                        e.Category.Event.OrganiserId == organiserId.Value)
            .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
            .Include(e => e.Participant)
            .ToListAsync();

        return Ok(enrolments);
    }

}
