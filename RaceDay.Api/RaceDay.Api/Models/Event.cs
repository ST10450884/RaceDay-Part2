
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Event
{
    public int EventId { get; set; }

    [Range(1, int.MaxValue)]
    public int OrganiserId { get; set; }

    [Required, MinLength(2), MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required, MaxLength(20)]
    public string EventType { get; set; } = "Running";

    public DateTime EventDate { get; set; }

    [Required, MaxLength(150)]
    public string Venue { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Province { get; set; } = string.Empty;

    public DateTime RegistrationCloseDate { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxParticipants { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Draft";

    public DateTime CreatedAtUtc { get; set; }

    public User? Organiser { get; set; }

    public Route? Route { get; set; }

    public ICollection<EventCategory> Categories { get; set; }
        = new List<EventCategory>();

    public ICollection<WeatherSnapshot> WeatherSnapshots { get; set; }
        = new List<WeatherSnapshot>();
}
