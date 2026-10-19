
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherSnapshotsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public WeatherSnapshotsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/WeatherSnapshots
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeatherSnapshot>>> GetWeatherSnapshots()
        {
            var snapshots = await _context.WeatherSnapshots
                .AsNoTracking()
                .ToListAsync();

            return Ok(snapshots);
        }

        // GET: api/WeatherSnapshots/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<WeatherSnapshot>> GetWeatherSnapshot(int id)
        {
            var snapshot = await _context.WeatherSnapshots
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WeatherSnapshotId == id);

            if (snapshot == null)
            {
                return NotFound(new
                {
                    message = $"Weather snapshot with ID {id} was not found."
                });
            }

            return Ok(snapshot);
        }
    }
}
