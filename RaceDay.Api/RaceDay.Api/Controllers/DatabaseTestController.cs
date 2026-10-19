
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using System;
using System.Threading.Tasks;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseTestController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public DatabaseTestController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                bool connected = await _context.Database.CanConnectAsync();

                if (connected)
                {
                    return Ok(new
                    {
                        message = "Successfully connected to RaceDayDB!"
                    });
                }

                return StatusCode(503, new
                {
                    message = "Could not connect to the database."
                });
            }
            catch (Exception)
            {
                return StatusCode(503, new
                {
                    message = "Database connection failed. Check the connection string and SQL Server."
                });
            }
        }
    }
}
