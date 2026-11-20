
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Authorization;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly BlobStorageService _blobStorage;

        public EventsController(
            RaceDayDbContext context,
            BlobStorageService blobStorage)
        {
            _context = context;
            _blobStorage = blobStorage;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound(new { message = $"Event with ID {id} was not found." });

            return Ok(eventItem);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            var events = await _context.Events
                .AsNoTracking()
                .ToListAsync();

            return Ok(events);
        }

        [SessionAuthorize("Organiser")]
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event eventItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var organiserId = HttpContext.Session.GetInt32("UserId");

            if (organiserId == null)
                return Unauthorized();

            if (eventItem.RegistrationCloseDate >= eventItem.EventDate)
                return BadRequest(new { message = "Registration close date must be before the event date." });

            if (eventItem.MaxParticipants <= 0)
                return BadRequest(new { message = "Maximum participants must be greater than zero." });

            var validEventTypes = new[] { "Running", "Walking", "Cycling" };

            if (!validEventTypes.Contains(eventItem.EventType))
                return BadRequest(new { message = "Event type must be Running, Walking, or Cycling." });

            eventItem.OrganiserId = organiserId.Value;
            eventItem.CreatedAtUtc = DateTime.UtcNow;

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = eventItem.EventId }, eventItem);
        }

        [SessionAuthorize("Organiser")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(int id, Event updatedEvent)
        {
            var organiserId = HttpContext.Session.GetInt32("UserId");

            if (organiserId == null)
                return Unauthorized();

            var existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null)
                return NotFound(new { message = $"Event with ID {id} was not found." });

            if (existingEvent.OrganiserId != organiserId.Value)
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (updatedEvent.RegistrationCloseDate >= updatedEvent.EventDate)
                return BadRequest(new { message = "Registration close date must be before the event date." });

            if (updatedEvent.MaxParticipants <= 0)
                return BadRequest(new { message = "Maximum participants must be greater than zero." });

            var validEventTypes = new[] { "Running", "Walking", "Cycling" };

            if (!validEventTypes.Contains(updatedEvent.EventType))
                return BadRequest(new { message = "Event type must be Running, Walking, or Cycling." });

            existingEvent.Name = updatedEvent.Name;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.EventType = updatedEvent.EventType;
            existingEvent.EventDate = updatedEvent.EventDate;
            existingEvent.Venue = updatedEvent.Venue;
            existingEvent.City = updatedEvent.City;
            existingEvent.Province = updatedEvent.Province;
            existingEvent.RegistrationCloseDate = updatedEvent.RegistrationCloseDate;
            existingEvent.MaxParticipants = updatedEvent.MaxParticipants;
            existingEvent.Status = updatedEvent.Status;

            string? oldPosterUrl = existingEvent.PosterUrl;

            if (!string.IsNullOrWhiteSpace(updatedEvent.PosterUrl))
                existingEvent.PosterUrl = updatedEvent.PosterUrl;

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(updatedEvent.PosterUrl) &&
                !string.IsNullOrWhiteSpace(oldPosterUrl) &&
                oldPosterUrl != updatedEvent.PosterUrl)
            {
                try
                {
                    string oldBlobName = Path.GetFileName(
                        oldPosterUrl.Split('?')[0]);

                    await _blobStorage.DeleteAsync(oldBlobName);
                }
                catch
                {
                    // The event update remains successful if old poster cleanup fails.
                }
            }

            return NoContent();
        }

        [SessionAuthorize("Organiser")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var organiserId = HttpContext.Session.GetInt32("UserId");

            if (organiserId == null)
                return Unauthorized();

            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
                return NotFound(new { message = $"Event with ID {id} was not found." });

            if (eventItem.OrganiserId != organiserId.Value)
                return Forbid();

            string? posterUrl = eventItem.PosterUrl;

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(posterUrl))
            {
                try
                {
                    string blobName = Path.GetFileName(
                        posterUrl.Split('?')[0]);

                    await _blobStorage.DeleteAsync(blobName);
                }
                catch
                {
                    // The event has already been deleted from the database.
                }
            }

            return NoContent();
        }
    }
}
