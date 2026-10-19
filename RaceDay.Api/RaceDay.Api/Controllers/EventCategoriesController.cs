
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Authorization;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventCategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventCategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/EventCategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventCategory>>> GetEventCategories()
        {
            var categories = await _context.EventCategories
                .AsNoTracking()
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/EventCategories/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventCategory>> GetEventCategory(int id)
        {
            var category = await _context.EventCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = $"Event category with ID {id} was not found."
                });
            }

            return Ok(category);
        }

        // POST: api/EventCategories
        [SessionAuthorize("Organiser")]
        [HttpPost]
        public async Task<ActionResult<EventCategory>> CreateEventCategory(
            EventCategory category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == category.EventId);

            if (!eventExists)
            {
                return BadRequest(new
                {
                    message = "The specified event does not exist."
                });
            }

            if (category.DistanceKm <= 0 ||
                category.Fee < 0 ||
                category.MinimumAge < 0 ||
                category.Capacity <= 0)
            {
                return BadRequest(new
                {
                    message = "Distance and capacity must be positive; fee and minimum age cannot be negative."
                });
            }

            category.CategoryId = 0;
            category.Event = null;

            _context.EventCategories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEventCategory),
                new { id = category.CategoryId },
                category);
        }

        // PUT: api/EventCategories/1
        [SessionAuthorize("Organiser")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEventCategory(
            int id,
            EventCategory updatedCategory)
        {
            var category = await _context.EventCategories.FindAsync(id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = $"Event category with ID {id} was not found."
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == updatedCategory.EventId);

            if (!eventExists)
            {
                return BadRequest(new
                {
                    message = "The specified event does not exist."
                });
            }

            if (updatedCategory.DistanceKm <= 0 ||
                updatedCategory.Fee < 0 ||
                updatedCategory.MinimumAge < 0 ||
                updatedCategory.Capacity <= 0)
            {
                return BadRequest(new
                {
                    message = "Distance and capacity must be positive; fee and minimum age cannot be negative."
                });
            }

            category.EventId = updatedCategory.EventId;
            category.Name = updatedCategory.Name;
            category.DistanceKm = updatedCategory.DistanceKm;
            category.Fee = updatedCategory.Fee;
            category.MinimumAge = updatedCategory.MinimumAge;
            category.Capacity = updatedCategory.Capacity;
            category.StartTime = updatedCategory.StartTime;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/EventCategories/1
        [SessionAuthorize("Organiser")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEventCategory(int id)
        {
            var category = await _context.EventCategories.FindAsync(id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = $"Event category with ID {id} was not found."
                });
            }

            _context.EventCategories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
