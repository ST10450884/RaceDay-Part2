
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }


        // GET: api/Events/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = $"Event with ID {id} was not found."
                });
            }

            return Ok(eventItem);
        }


        // GET: api/Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            var events = await _context.Events
                .AsNoTracking()
                .ToListAsync();

            return Ok(events);
        }

        // POST: api/Events
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event eventItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (eventItem.RegistrationCloseDate >= eventItem.EventDate)
            {
                return BadRequest(new
                {
                    message = "Registration close date must be before the event date."
                });
            }

            if (eventItem.MaxParticipants <= 0)
            {
                return BadRequest(new
                {
                    message = "Maximum participants must be greater than zero."
                });
            }

            var validEventTypes = new[] { "Running", "Walking", "Cycling" };

            if (!validEventTypes.Contains(eventItem.EventType))
            {
                return BadRequest(new
                {
                    message = "Event type must be Running, Walking, or Cycling."
                });
            }

            // Use the database-generated creation timestamp.
            eventItem.CreatedAtUtc = DateTime.UtcNow;

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = eventItem.EventId },
                eventItem);
        }

    }
}
