
namespace RaceDay.Mvc.Models
{
    public class EnrolmentViewModel
    {
        public int EnrolmentId { get; set; }
        public int CategoryId { get; set; }
        public int ParticipantId { get; set; }
        public DateTime EnrolledAtUtc { get; set; }
        public string Status { get; set; } = "";
        public string EmergencyContactName { get; set; } = "";
        public string EmergencyContactPhone { get; set; } = "";

        public EnrolmentCategoryViewModel? Category { get; set; }
    }

    public class EnrolmentCategoryViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public decimal Fee { get; set; }
        public int MinimumAge { get; set; }
        public TimeSpan StartTime { get; set; }

        public EnrolmentEventViewModel? Event { get; set; }
    }

    public class EnrolmentEventViewModel
    {
        public int EventId { get; set; }
        public string Name { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string Venue { get; set; } = "";
        public string City { get; set; } = "";
        public string Province { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
