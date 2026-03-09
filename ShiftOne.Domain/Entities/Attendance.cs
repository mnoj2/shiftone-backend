using System;

namespace ShiftOne.Domain.Entities
{
    public class Attendance
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? SignInTime { get; set; }
        public DateTime? SignOffTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public double? TotalHours { get; set; }
    }
}
