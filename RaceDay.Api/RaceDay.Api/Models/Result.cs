
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Result
{
    public int ResultId { get; set; }

    public int EnrolmentId { get; set; }

    public int? FinishTimeSeconds { get; set; }

    public int? OverallPosition { get; set; }

    public int? CategoryPosition { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Finished";

    public DateTime RecordedAtUtc { get; set; }

    public Enrolment? Enrolment { get; set; }
}
