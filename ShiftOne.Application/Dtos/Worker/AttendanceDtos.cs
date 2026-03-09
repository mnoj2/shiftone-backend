using System;
using System.Collections.Generic;

namespace ShiftOne.Application.Dtos.Worker
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

    public class MonthlySummaryDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public double TotalHours { get; set; }
    }

    public class WeeklyAttendanceDto
    {
        public string Day { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DailyHoursDto
    {
        public string Date { get; set; } = string.Empty;
        public double Hours { get; set; }
    }

    public class TopWorkerDto
    {
        public string Name { get; set; } = string.Empty;
        public double Hours { get; set; }
    }

    public class ShiftDurationDistributionDto
    {
        public int Undertime { get; set; }
        public int Standard { get; set; }
    }

    public class SupervisorAnalyticsDto
    {
        public List<WeeklyAttendanceDto> WeeklyTrend { get; set; } = new();
        public List<DailyHoursDto> MonthlyHours { get; set; } = new();
        public List<TopWorkerDto> TopWorkers { get; set; } = new();
        public ShiftDurationDistributionDto ShiftDuration { get; set; } = new();
    }

    public class RecentActivityDto
    {
        public string WorkerName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime Time { get; set; }
    }

    public class SupervisorDashboardStatsDto
    {
        public int TotalPresent { get; set; }
        public int ActiveWorkers { get; set; }
        public double TotalHoursToday { get; set; }
        public double MonthlyTotalHours { get; set; }
        public double MonthlyAvgAttendance { get; set; }
        public int MonthlyPeakAttendance { get; set; }
        public string TopPerformerName { get; set; } = "N/A";
        public double TopPerformerHours { get; set; }
        public string RunnerUpName { get; set; } = "N/A";
        public double RunnerUpHours { get; set; }
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
    }

    public class SupervisorHomeDto
    {
        public int TotalWorkers { get; set; }
        public int CompletedShifts { get; set; }
        public int InProgressShifts { get; set; }
        public int ActiveWorkers { get; set; }
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
