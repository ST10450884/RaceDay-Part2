
namespace RaceDay.Mvc.Models
{
    public class ParticipantDashboardViewModel
    {
        public string ParticipantName { get; set; } = "Participant";

        public int TotalEvents { get; set; }

        public int UpcomingEvents { get; set; }

        public int CompletedEvents { get; set; }
    }
}
