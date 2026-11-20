
namespace RaceDay.Mvc.Models
{
    public class ResultViewModel
    {
        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }

        public int? FinishTimeSeconds { get; set; }

        public int? OverallPosition { get; set; }

        public int? CategoryPosition { get; set; }

        public string Status { get; set; } = "Finished";

        public DateTime RecordedAtUtc { get; set; }

        public int? ParticipantId { get; set; }

        public string ParticipantEmail { get; set; } = "";

        public int? CategoryId { get; set; }

        public string CategoryName { get; set; } = "";

        public int? EventId { get; set; }

        public string EventName { get; set; } = "";
    }
}
