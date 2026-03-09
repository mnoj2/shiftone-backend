using System;

namespace ShiftOne.Application.Dtos.Worker
{
    public class ShiftDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class AttendanceSummaryDto
    {
        public bool SignedIn { get; set; }
        public bool SignedOff { get; set; }
        public double? TotalHours { get; set; }
    }

    public class WorkerHomeDto
    {
        public ShiftDto? TodayShift { get; set; }
        public AttendanceSummaryDto? Attendance { get; set; }
        public bool HasPendingAutoSignOff { get; set; }
    }
}
