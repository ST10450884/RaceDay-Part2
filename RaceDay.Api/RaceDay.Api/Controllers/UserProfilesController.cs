
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfilesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UserProfilesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/UserProfiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserProfile>>> GetUserProfiles()
        {
            var profiles = await _context.UserProfiles
                .AsNoTracking()
                .ToListAsync();

            return Ok(profiles);
        }

        // GET: api/UserProfiles/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserProfile>> GetUserProfile(int id)
        {
            var profile = await _context.UserProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == id);

            if (profile == null)
            {
                return NotFound(new
                {
                    message = $"User profile with ID {id} was not found."
                });
            }

            return Ok(profile);
        }
    }
}
