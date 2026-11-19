
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models
{
    public class CreateEnrolmentViewModel
    {
        public int CategoryId { get; set; }

        public int EventId { get; set; }

        public string CategoryName { get; set; } = "";

        [Required(ErrorMessage = "Emergency contact name is required.")]
        [MaxLength(160)]
        public string EmergencyContactName { get; set; } = "";

        [Required(ErrorMessage = "Emergency contact phone is required.")]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [MaxLength(20)]
        public string EmergencyContactPhone { get; set; } = "";
    }
}
