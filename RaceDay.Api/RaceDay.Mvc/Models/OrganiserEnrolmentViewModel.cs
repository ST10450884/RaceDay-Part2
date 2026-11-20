
namespace RaceDay.Mvc.Models
{
    public class OrganiserEnrolmentViewModel
    {
        public int EnrolmentId { get; set; }
        public int CategoryId { get; set; }
        public int ParticipantId { get; set; }
        public DateTime EnrolledAtUtc { get; set; }
        public string Status { get; set; } = "";
        public string EmergencyContactName { get; set; } = "";
        public string EmergencyContactPhone { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string EventName { get; set; } = "";
    }
}
