
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(80)]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(80)]
        public string LastName { get; set; } = "";

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
    }
}
