using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ShiftOne.Application.Dtos {
    public class LocationDto {
        [Required]
        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [Required]
        [JsonPropertyName("lng")]
        public double Longitude { get; set; }
    }

    public class ManualSignOffDto {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public DateTime SignOffTime { get; set; }
    }

    public class ShiftDto {
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class AttendanceSummaryDto {
        [Required]
        public bool SignedIn { get; set; }

        [Required]
        public bool SignedOff { get; set; }

        public double? TotalHours { get; set; }
    }

    public class WorkerHomeDto {
        public ShiftDto? TodayShift { get; set; }
        public AttendanceSummaryDto? Attendance { get; set; }

        [Required]
        public bool HasPendingAutoSignOff { get; set; }
    }
}