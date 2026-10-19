using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using Microsoft.EntityFrameworkCore;
using Route = RaceDay.Api.Models.Route;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoutesController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public RoutesController(RaceDayDbContext context)
    {
        _context = context;
    }

    // GET: api/Routes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Route>>> GetRoutes()
    {
        var routes = await _context.Routes
            .AsNoTracking()
            .ToListAsync();

        return Ok(routes);
    }

    // GET: api/Routes/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Route>> GetRoute(int id)
    {
        var route = await _context.Routes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RouteId == id);

        if (route == null)
        {
            return NotFound();
        }

        return Ok(route);
    }
}
