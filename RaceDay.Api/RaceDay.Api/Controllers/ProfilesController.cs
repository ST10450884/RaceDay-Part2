
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Authorization;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[SessionAuthorize]
public class ProfilesController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ProfilesController(RaceDayDbContext context)
    {
        _context = context;
    }

    // GET: api/Profiles/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        var profile = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            return NotFound(new
            {
                message = "Profile not found. Please create your profile."
            });
        }

        return Ok(profile);
    }

    // POST: api/Profiles
    [HttpPost]
    public async Task<IActionResult> CreateProfile(UserProfile profile)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = HttpContext.Session.GetInt32("UserId")!.Value;

        var profileExists = await _context.UserProfiles
            .AnyAsync(p => p.UserId == userId);

        if (profileExists)
        {
            return Conflict(new
            {
                message = "You already have a profile."
            });
        }

        profile.UserId = userId;
        profile.UserProfileId = 0;
        profile.User = null;

        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMyProfile),
            new { },
            profile);
    }

    // PUT: api/Profiles/me
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(UserProfile updatedProfile)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = HttpContext.Session.GetInt32("UserId")!.Value;

        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            return NotFound(new
            {
                message = "Profile not found. Create your profile first."
            });
        }

        profile.FirstName = updatedProfile.FirstName;
        profile.LastName = updatedProfile.LastName;
        profile.PhoneNumber = updatedProfile.PhoneNumber;
        profile.DateOfBirth = updatedProfile.DateOfBirth;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
