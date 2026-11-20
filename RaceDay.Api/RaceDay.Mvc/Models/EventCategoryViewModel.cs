
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models
{
    public class EventCategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, 1000)]
        public decimal DistanceKm { get; set; }

        [Range(0, 100000)]
        public decimal Fee { get; set; }

        [Range(0, 120)]
        public int MinimumAge { get; set; }

        [Range(1, 100000)]
        public int Capacity { get; set; }

        public TimeSpan StartTime { get; set; }

        public string EventName { get; set; } = string.Empty;
    }
}
