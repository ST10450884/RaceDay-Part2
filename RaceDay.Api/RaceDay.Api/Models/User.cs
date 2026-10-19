
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class User
{
    public int UserId { get; set; }

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "Participant";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public UserProfile? UserProfile { get; set; }

    public ICollection<Event> OrganisedEvents { get; set; }
        = new List<Event>();

    public ICollection<Enrolment> Enrolments { get; set; }
        = new List<Enrolment>();
}
