
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class WeatherSnapshot
{
    public int WeatherSnapshotId { get; set; }

    public int EventId { get; set; }

    public DateTime ObservedAtUtc { get; set; }

    public decimal? TemperatureC { get; set; }

    public decimal? WindSpeedKph { get; set; }

    public byte? PrecipitationChance { get; set; }

    [MaxLength(100)]
    public string? Conditions { get; set; }

    public Event? Event { get; set; }
}
