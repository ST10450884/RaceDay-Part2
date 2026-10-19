
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Enrolment
{
    public int EnrolmentId { get; set; }

    public int CategoryId { get; set; }

    public int ParticipantId { get; set; }

    public DateTime EnrolledAtUtc { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [Required, MaxLength(160)]
    public string EmergencyContactName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string EmergencyContactPhone { get; set; } = string.Empty;

    public EventCategory? Category { get; set; }

    public User? Participant { get; set; }

    public Result? Result { get; set; }
}
