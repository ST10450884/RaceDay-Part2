using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class CreateEnrolmentRequest
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Required, MaxLength(160)]
    public string EmergencyContactName { get; set; } = string.Empty;

    [Required, Phone, MaxLength(20)]
    public string EmergencyContactPhone { get; set; } = string.Empty;
}
