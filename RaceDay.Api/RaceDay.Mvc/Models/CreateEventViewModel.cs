
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models
{
    public class CreateEventViewModel
    {
        [Required]
        [MinLength(2)]
        [MaxLength(150)]
        public string Name { get; set; } = "";

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string EventType { get; set; } = "Running";

        [Required]
        [DataType(DataType.Date)]
        public DateTime? EventDate { get; set; }

        [Required]
        [MaxLength(150)]
        public string Venue { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string Province { get; set; } = "";

        [Required]
        [DataType(DataType.Date)]
        public DateTime? RegistrationCloseDate { get; set; }

        [Range(1, int.MaxValue)]
        public int MaxParticipants { get; set; } = 100;

        [Required]
        public string Status { get; set; } = "Draft";

        public IFormFile? PosterFile { get; set; }

        public string? PosterUrl { get; set; }
    }
}
