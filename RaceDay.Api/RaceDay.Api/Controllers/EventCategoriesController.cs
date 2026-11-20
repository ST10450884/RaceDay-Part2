
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
        // Public: View all event categories.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventCategory>>> GetEventCategories()
        {
            var categories = await _context.EventCategories
                .AsNoTracking()
                .Include(c => c.Event)
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/EventCategories/1
        // Public: View one category.
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventCategory>> GetEventCategory(int id)
        {
            var category = await _context.EventCategories
                .AsNoTracking()
                .Include(c => c.Event)
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

        // GET: api/EventCategories/organiser
        // Organiser: View categories belonging to their events.
        [HttpGet("organiser")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> GetOrganiserEventCategories()
        {
            var organiserId = HttpContext.Session.GetInt32("UserId");

            if (organiserId == null)
                return Unauthorized();

            var categories = await _context.EventCategories
                .AsNoTracking()
                .Include(c => c.Event)
                .Where(c => c.Event != null &&
                            c.Event.OrganiserId == organiserId.Value)
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/EventCategories
        // Organiser: Create a category for their own event.
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<ActionResult<EventCategory>> CreateEventCategory(
            EventCategory category)
        {
            var organiserId = HttpContext.Session.GetInt32("UserId");

            if (organiserId == null)
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify that the event belongs to this organiser.
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e =>
                    e.EventId == category.EventId &&
                    e.OrganiserId == organiserId.Value);

            if (eventEntity == null)
            {
                return NotFound(new
                {
                    message = "The specified event was not found or does not belong to you."
                });
            }

            // Validate category values.
            if (category.DistanceKm <= 0 ||
                category.Fee < 0 ||
                category.MinimumAge < 0 ||
                category.Capacity <= 0 ||
                category.StartTime < TimeSpan.Zero ||
                category.StartTime >= TimeSpan.FromDays(1))
            {
                return BadRequest(new
                {
                    message = "Enter a valid distance, fee, minimum age, capacity and start time."
                });
            }

            // Prevent duplicate category names for the same event.
            var categoryName = category.Name.Trim();

            var duplicateCategory = await _context.EventCategories
                .AnyAsync(c =>
                    c.EventId == category.EventId &&
                    c.Name.ToLower() == categoryName.ToLower());

            if (duplicateCategory)
            {
                return Conflict(new
                {
                    message = "A category with this name already exists for this event."
                });
            }

            category.CategoryId = 0;
            category.Name = categoryName;
            category.Event = null;

            _context.EventCategories.Add(category);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEventCategory),
                new { id = category.CategoryId },
                category);
        }

        // PUT: api/EventCategories/1
        // Organiser: Update a category belonging to their own event.
        [HttpPut("{id:int}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> UpdateEventCategory(
            int id,
            EventCategory updatedCategory)
        {
            var organiserId = HttpContext.Session.GetInt32("UserId");

            if (organiserId == null)
                return Unauthorized();

            if (id != updatedCategory.CategoryId)
            {
                return BadRequest(new
                {
                    message = "The category ID in the URL does not match the submitted category."
                });
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find the existing category.
            var category = await _context.EventCategories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = $"Event category with ID {id} was not found."
                });
            }

            // Verify ownership.
            if (category.Event == null ||
                category.Event.OrganiserId != organiserId.Value)
            {
                return Forbid();
            }

            // Verify that the target event belongs to this organiser.
            var targetEvent = await _context.Events
                .FirstOrDefaultAsync(e =>
                    e.EventId == updatedCategory.EventId &&
                    e.OrganiserId == organiserId.Value);

            if (targetEvent == null)
            {
                return BadRequest(new
                {
                    message = "The specified event does not exist or does not belong to you."
                });
            }

            // Validate updated values.
            if (updatedCategory.DistanceKm <= 0 ||
                updatedCategory.Fee < 0 ||
                updatedCategory.MinimumAge < 0 ||
                updatedCategory.Capacity <= 0 ||
                updatedCategory.StartTime < TimeSpan.Zero ||
                updatedCategory.StartTime >= TimeSpan.FromDays(1))
            {
                return BadRequest(new
                {
                    message = "Enter a valid distance, fee, minimum age, capacity and start time."
                });
            }

            // Prevent duplicate names, excluding the current category.
            var categoryName = updatedCategory.Name.Trim();

            var duplicateCategory = await _context.EventCategories
                .AnyAsync(c =>
                    c.CategoryId != id &&
                    c.EventId == updatedCategory.EventId &&
                    c.Name.ToLower() == categoryName.ToLower());

            if (duplicateCategory)
            {
                return Conflict(new
                {
                    message = "A category with this name already exists for this event."
                });
            }

            // Update category properties.
            category.EventId = updatedCategory.EventId;
            category.Name = categoryName;
            category.DistanceKm = updatedCategory.DistanceKm;
            category.Fee = updatedCategory.Fee;
            category.MinimumAge = updatedCategory.MinimumAge;
            category.Capacity = updatedCategory.Capacity;
            category.StartTime = updatedCategory.StartTime;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event category updated successfully."
            });
        }

        // DELETE: api/EventCategories/1
        // Organiser: Delete a category belonging to their own event.
        [HttpDelete("{id:int}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> DeleteEventCategory(int id)
        {
            var organiserId = HttpContext.Session.GetInt32("UserId");

            if (organiserId == null)
                return Unauthorized();

            var category = await _context.EventCategories
                .Include(c => c.Event)
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = $"Event category with ID {id} was not found."
                });
            }

            // Verify ownership.
            if (category.Event == null ||
                category.Event.OrganiserId != organiserId.Value)
            {
                return Forbid();
            }

            // Protect existing participant enrolments.
            if (category.Enrolments.Any())
            {
                return Conflict(new
                {
                    message = "This category cannot be deleted because participants are already enrolled in it."
                });
            }

            _context.EventCategories.Remove(category);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event category deleted successfully."
            });
        }
    }
}
