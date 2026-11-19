
namespace RaceDay.Mvc.Models
{
    public class EventCategoryViewModel
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public decimal Fee { get; set; }
        public int MinimumAge { get; set; }
        public int Capacity { get; set; }
        public TimeSpan StartTime { get; set; }
    }
}
