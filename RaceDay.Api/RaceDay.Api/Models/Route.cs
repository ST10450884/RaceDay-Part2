
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Route
{
    public int RouteId { get; set; }

    public int EventId { get; set; }

    public decimal DistanceKm { get; set; }

    public decimal? StartLatitude { get; set; }

    public decimal? StartLongitude { get; set; }

    public decimal? EndLatitude { get; set; }

    public decimal? EndLongitude { get; set; }

    [MaxLength(500)]
    public string? RouteMapUrl { get; set; }

    public Event? Event { get; set; }
}
