using System;
using System.Collections.Generic;

namespace ShiftOne.Application.Dtos
{
    public class AttendanceRecordDto
    {
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public DateTime? SignInTime { get; set; }
        public DateTime? SignOffTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public double? TotalHours { get; set; }
    }

    public class DailyHoursDto
    {
        public string Date { get; set; } = string.Empty;
        public double Hours { get; set; }
    }

    public class SupervisorAnalyticsDto
    {
        public List<DailyHoursDto> MonthlyHours { get; set; } = new();
    }

    public class SupervisorHomeDto
    {
        public int TotalWorkers { get; set; }
        public int CompletedShifts { get; set; }
        public int InProgressShifts { get; set; }
    }

    public class WorkerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsWorking { get; set; }
    }
}
