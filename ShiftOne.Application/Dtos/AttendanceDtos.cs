using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Application.Dtos {
    public class AttendanceRecordDto {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string WorkerName { get; set; } = string.Empty;
        public DateTime? SignInTime { get; set; }
        public DateTime? SignOffTime { get; set; }
        [Required]
        public string Status { get; set; } = string.Empty;
        public double? TotalHours { get; set; }
    }

    public class AttendanceInfoDto {
        [Required]
        public string Status { get; set; } = string.Empty;
        public DateTime? SignInTime { get; set; }
        public DateTime? SignOffTime { get; set; }
        public double? TotalHours { get; set; }
    }

    public class DailyHoursDto {
        [Required]
        public string Date { get; set; } = string.Empty;
        [Required]
        public double Hours { get; set; }
    }

    public class SupervisorAnalyticsDto {
        [Required]
        public List<DailyHoursDto> MonthlyHours { get; set; } = new();
    }

    public class SupervisorHomeDto {
        [Required]
        public int TotalWorkers { get; set; }
        [Required]
        public int CompletedShifts { get; set; }
        [Required]
        public int InProgressShifts { get; set; }
    }

    public class WorkerDto {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;
        [Required]
        public bool IsWorking { get; set; }
    }
}