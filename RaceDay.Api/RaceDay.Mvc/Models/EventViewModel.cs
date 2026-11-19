
namespace RaceDay.Mvc.Models
{
    public class EventViewModel
    {
        public int EventId { get; set; }

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public string EventType { get; set; } = "";

        public DateTime EventDate { get; set; }

        public string Venue { get; set; } = "";

        public string City { get; set; } = "";

        public string Province { get; set; } = "";

        public DateTime RegistrationCloseDate { get; set; }

        public int MaxParticipants { get; set; }

        public string Status { get; set; } = "";
    }
}
