
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class UserProfile
{
    public int UserProfileId { get; set; }

    public int UserId { get; set; }

    [Required, MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public User? User { get; set; }
}
