
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class EventCategory
{
    public int CategoryId { get; set; }

    public int EventId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public decimal DistanceKm { get; set; }

    public decimal Fee { get; set; }

    public int MinimumAge { get; set; }

    public int Capacity { get; set; }

    public TimeSpan StartTime { get; set; }

    public Event? Event { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; }
        = new List<Enrolment>();
}
