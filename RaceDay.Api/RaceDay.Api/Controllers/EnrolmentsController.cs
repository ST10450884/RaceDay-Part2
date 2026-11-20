
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Authorization;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.DTOs;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        // Participant views their own enrolments.
        [HttpGet]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetEnrolments()
        {
            var participantId = HttpContext.Session.GetInt32("UserId");

            if (participantId == null)
                return Unauthorized();

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.ParticipantId == participantId.Value)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.CategoryId,
                    e.ParticipantId,
                    e.EnrolledAtUtc,
                    e.Status,
                    e.EmergencyContactName,
                    e.EmergencyContactPhone,

                    Category = e.Category == null ? null : new
                    {
                        e.Category.CategoryId,
                        e.Category.Name,
                        e.Category.DistanceKm,
                        e.Category.Fee,
                        e.Category.MinimumAge,
                        e.Category.StartTime,

                        Event = e.Category.Event == null ? null : new
                        {
                            e.Category.Event.EventId,
                            e.Category.Event.Name,
                            e.Category.Event.EventDate,
                            e.Category.Event.Venue,
                            e.Category.Event.City,
                            e.Category.Event.Province,
                            e.Category.Event.Status
                        }
                    }
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/Enrolments/1
        // Participant views one of their own enrolments.
        [HttpGet("{id:int}")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            var participantId = HttpContext.Session.GetInt32("UserId");

            if (participantId == null)
                return Unauthorized();

            var enrolment = await _context.Enrolments
                .AsNoTracking()
                .Where(e =>
                    e.EnrolmentId == id &&
                    e.ParticipantId == participantId.Value)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.CategoryId,
                    e.ParticipantId,
                    e.EnrolledAtUtc,
                    e.Status,
                    e.EmergencyContactName,
                    e.EmergencyContactPhone,

                    Category = e.Category == null ? null : new
                    {
                        e.Category.CategoryId,
                        e.Category.Name,
                        e.Category.DistanceKm,
                        e.Category.Fee,
                        e.Category.MinimumAge,
                        e.Category.StartTime,

                        Event = e.Category.Event == null ? null : new
                        {
                            e.Category.Event.EventId,
                            e.Category.Event.Name,
                            e.Category.Event.EventDate,
                            e.Category.Event.Venue,
                            e.Category.Event.City,
                            e.Category.Event.Province,
                            e.Category.Event.Status
                        }
                    }
                })
                .FirstOrDefaultAsync();

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
        // Participant views their own enrolments.
        [HttpGet("my")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            return await GetEnrolments();
        }

        // POST: api/Enrolments
        // Participant creates an enrolment.
        [HttpPost]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> CreateEnrolment(
            CreateEnrolmentRequest request)
        {
            var participantId = HttpContext.Session.GetInt32("UserId");

            if (participantId == null)
                return Unauthorized();

            var category = await _context.EventCategories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == request.CategoryId);

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
                .FirstOrDefaultAsync(p =>
                    p.UserId == participantId.Value);

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
                EmergencyContactName =
                    request.EmergencyContactName.Trim(),
                EmergencyContactPhone =
                    request.EmergencyContactPhone.Trim(),
                EnrolledAtUtc = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.Enrolments.Add(enrolment);

            await _context.SaveChangesAsync();

            var response = new
            {
                enrolment.EnrolmentId,
                enrolment.CategoryId,
                enrolment.ParticipantId,
                enrolment.EnrolledAtUtc,
                enrolment.Status,
                enrolment.EmergencyContactName,
                enrolment.EmergencyContactPhone,
                CategoryName = category.Name,
                EventName = category.Event.Name
            };

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.EnrolmentId },
                response);
        }

        // GET: api/Enrolments/organiser
        // Organiser views enrolments for their own events.
        [HttpGet("organiser")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> GetOrganiserEnrolments()
        {
            var organiserId = HttpContext.Session.GetInt32("UserId");

            if (organiserId == null)
                return Unauthorized();

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e =>
                    e.Category != null &&
                    e.Category.Event != null &&
                    e.Category.Event.OrganiserId == organiserId.Value)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.CategoryId,
                    e.ParticipantId,
                    e.EnrolledAtUtc,
                    e.Status,
                    e.EmergencyContactName,
                    e.EmergencyContactPhone,

                    CategoryName = e.Category!.Name,
                    EventName = e.Category.Event!.Name
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // PUT: api/Enrolments/1/status
        // Organiser updates enrolment status for their own events.
        [HttpPut("{id:int}/status")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> UpdateEnrolmentStatus(
            int id,
            UpdateEnrolmentStatusRequest request)
        {
            var organiserId = HttpContext.Session.GetInt32("UserId");

            if (organiserId == null)
                return Unauthorized();

            var enrolment = await _context.Enrolments
                .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment was not found."
                });
            }

            // Ensure the organiser owns the event.
            if (enrolment.Category?.Event?.OrganiserId != organiserId.Value)
                return Forbid();

            // These values must match the SQL CHECK constraint.
            var validStatuses = new[]
            {
                "Pending",
                "Confirmed",
                "Cancelled",
                "Disqualified"
            };

            if (string.IsNullOrWhiteSpace(request.Status) ||
                !validStatuses.Contains(request.Status))
            {
                return BadRequest(new
                {
                    message = "Status must be Pending, Confirmed, Cancelled, or Disqualified."
                });
            }

            enrolment.Status = request.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                enrolment.EnrolmentId,
                enrolment.Status,
                message = "Enrolment status updated successfully."
            });
        }
    }

    // Request model for updating enrolment status.
    public class UpdateEnrolmentStatusRequest
    {
        public string Status { get; set; } = "";
    }
}
