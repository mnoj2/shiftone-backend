using System;

namespace ShiftOne.Domain.Models
{
    public class AttendanceSummary
    {
        public bool SignedIn { get; set; }
        public bool SignedOff { get; set; }
        public double? TotalHours { get; set; }
    }
}
