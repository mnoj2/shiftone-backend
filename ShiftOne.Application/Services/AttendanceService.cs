using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ShiftOne.Application.Dtos.Worker;
using ShiftOne.Application.Interfaces;
using ShiftOne.Application.Utils;
using ShiftOne.Domain.Entities;
using ShiftOne.Domain.Interfaces;
using ShiftOne.Domain.Interfaces.Common;

namespace ShiftOne.Application.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;

        public AttendanceService(IAttendanceRepository repo, IUserRepository userRepo, IConfiguration config)
        {
            _repo = repo;
            _userRepo = userRepo;
            _config = config;
        }

        private bool ValidateLocation(double lat, double lng)
        {
            var factoryLat = _config.GetValue<double>("FactoryLocation:Latitude");
            var factoryLng = _config.GetValue<double>("FactoryLocation:Longitude");
            var allowedRadius = _config.GetValue<double>("FactoryLocation:RadiusMeters", 200);

            var distance = LocationUtils.CalculateDistance(lat, lng, factoryLat, factoryLng);
            return distance <= allowedRadius;
        }

        private DateTime GetFactoryNow()
        {
            var tzId = _config.GetValue<string>("FactoryLocation:TimeZone") ?? "India Standard Time";
            var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }

        private DateTime GetFactoryLocalDate() => GetFactoryNow().Date;

        public async Task<string?> SignInAsync(int userId, double lat, double lng)
        {
            if (!ValidateLocation(lat, lng)) return null;

            var today = GetFactoryLocalDate();
            if (await _repo.HasAttendanceAsync(userId, today)) return null;

            var record = new Attendance
            {
                UserId = userId,
                Date = today,
                SignInTime = DateTime.UtcNow,
                Status = "SignedIn"
            };

            return await _repo.AddAsync(record) ? "Signed In Successfully" : null;
        }

        public async Task<string?> SignOffAsync(int userId, double lat, double lng)
        {
            if (!ValidateLocation(lat, lng)) return null;

            var today = GetFactoryLocalDate();
            var record = await _repo.GetActiveShiftAsync(userId, today);
            if (record == null) return null;

            record.SignOffTime = DateTime.UtcNow;
            if (record.SignInTime.HasValue)
            {
                double totalMinutes = (record.SignOffTime.Value - record.SignInTime.Value).TotalMinutes;
                record.TotalHours = Math.Max(0, totalMinutes / 60.0);
            }
            record.Status = "SignedOff";

            return await _repo.UpdateAsync(record) ? "Signed Off Successfully" : null;
        }

        public async Task<AttendanceInfoDto?> GetTodayInfoAsync(int userId)
        {
            var today = GetFactoryLocalDate();
            var record = await _repo.GetTodayRecordAsync(userId, today);

            if (record == null)
            {
                return new AttendanceInfoDto
                {
                    Date = today,
                    Status = "Not Started",
                    Message = "You haven't signed in today."
                };
            }

            double? displayHours = record.TotalHours;
            if (record.Status == "SignedIn" && record.SignInTime.HasValue)
            {
                var now = DateTime.UtcNow;
                double rawMinutes = (now - record.SignInTime.Value).TotalMinutes;
                displayHours = Math.Max(0, rawMinutes / 60.0);
            }

            return new AttendanceInfoDto
            {
                Date = record.Date,
                Status = record.Status,
                SignInTime = record.SignInTime,
                SignOffTime = record.SignOffTime,
                TotalHours = displayHours,
                Message = "Record fetched successfully."
            };
        }

        public async Task<List<WorkerDto>?> GetAllWorkersAsync()
        {
            var workers = await _userRepo.GetAllWorkersAsync();
            if (workers == null) return null;

            var today = GetFactoryLocalDate();
            var result = new List<WorkerDto>();
            foreach (var w in workers)
            {
                var isActive = (await _repo.GetActiveShiftAsync(w.Id, today)) != null;
                result.Add(new WorkerDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Email = w.Email,
                    Phone = w.Phone,
                    IsWorking = isActive
                });
            }
            return result;
        }

        public async Task<List<AttendanceRecordDto>?> GetWorkerHistoryAsync(int userId)
        {
            var data = await _repo.GetUserHistoryAsync(userId);
            if (data == null) return null;

            var worker = await _userRepo.GetByIdAsync(userId);
            var name = worker?.Name ?? "Unknown";

            return data.Select(r => new AttendanceRecordDto
            {
                Date = r.Date,
                UserId = r.UserId,
                WorkerName = name,
                SignInTime = r.SignInTime,
                SignOffTime = r.SignOffTime,
                Status = r.Status,
                TotalHours = r.TotalHours
            }).ToList();
        }

        public async Task<List<AttendanceRecordDto>?> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            var data = await _repo.GetByDateRangeAsync(start, end);
            if (data == null) return null;

            var workers = await _userRepo.GetAllWorkersAsync();
            return data.Select(r => new AttendanceRecordDto
            {
                Date = r.Date,
                UserId = r.UserId,
                WorkerName = workers?.FirstOrDefault(w => w.Id == r.UserId)?.Name ?? "Unknown",
                SignInTime = r.SignInTime,
                SignOffTime = r.SignOffTime,
                Status = r.Status,
                TotalHours = r.TotalHours
            }).ToList();
        }

        public async Task<List<MonthlySummaryDto>?> GetMonthlySummaryAsync(int month, int year)
        {
            var workers = await _userRepo.GetAllWorkersAsync();
            if (workers == null) return null;

            var records = await _repo.GetMonthlyRecordsAsync(month, year);
            if (records == null) return null;

            var daysInMonth = DateTime.DaysInMonth(year, month);
            var now = GetFactoryNow().Date;
            var monthStartDate = new DateTime(year, month, 1);
            var isCurrentMonth = (month == now.Month && year == now.Year);

            DateTime monthEndDate = isCurrentMonth ? now : new DateTime(year, month, daysInMonth);
            if (monthStartDate > now) monthEndDate = monthStartDate.AddDays(-1);

            int expectedWorkingDays = 0;
            for (DateTime d = monthStartDate; d <= monthEndDate; d = d.AddDays(1))
            {
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday) expectedWorkingDays++;
            }

            return workers.Select(user =>
            {
                var wRecords = records.Where(r => r.UserId == user.Id).ToList();
                var present = wRecords.Select(r => r.Date.Date).Distinct().Count();
                var hours = wRecords.Sum(r => r.TotalHours ?? 0);
                return new MonthlySummaryDto
                {
                    UserId = user.Id,
                    UserName = user.Name,
                    Email = user.Email,
                    PresentDays = present,
                    AbsentDays = Math.Max(0, expectedWorkingDays - present),
                    TotalHours = Math.Round(hours, 2)
                };
            }).OrderBy(x => x.UserName).ToList();
        }

        public async Task<SupervisorAnalyticsDto?> GetSupervisorAnalyticsAsync(int month, int year)
        {
            var records = await _repo.GetMonthlyRecordsAsync(month, year);
            if (records == null) return null;

            var workers = await _userRepo.GetAllWorkersAsync() ?? new List<User>();

            var startDate = new DateTime(year, month, 1);
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var weeklyTrend = new List<WeeklyAttendanceDto>();

            for (int i = 0; i < daysInMonth; i++)
            {
                var d = startDate.AddDays(i);
                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday) continue;
                var count = records.Where(r => r.Date.Date == d.Date).Select(r => r.UserId).Distinct().Count();
                weeklyTrend.Add(new WeeklyAttendanceDto { Day = d.Day.ToString(), Count = count });
            }

            var monthlyHours = records.GroupBy(r => r.Date).Select(g => new DailyHoursDto
            {
                Date = g.Key.ToString("dd MMM"),
                Hours = Math.Round(g.Sum(r => r.TotalHours ?? 0), 2)
            }).OrderBy(x => x.Date).ToList();

            var topWorkers = records.GroupBy(r => r.UserId).Select(g => new TopWorkerDto
            {
                Name = workers.FirstOrDefault(w => w.Id == g.Key)?.Name ?? "Unknown",
                Hours = Math.Round(g.Sum(r => r.TotalHours ?? 0), 2)
            }).OrderByDescending(x => x.Hours).Take(5).ToList();

            var finished = records.Where(r => r.TotalHours != null).ToList();
            var shiftDuration = new ShiftDurationDistributionDto
            {
                Undertime = finished.Count(r => r.TotalHours < 8.0),
                Standard = finished.Count(r => r.TotalHours >= 8.0)
            };

            return new SupervisorAnalyticsDto
            {
                WeeklyTrend = weeklyTrend,
                MonthlyHours = monthlyHours,
                TopWorkers = topWorkers,
                ShiftDuration = shiftDuration
            };
        }

        public async Task<SupervisorDashboardStatsDto?> GetSupervisorStatsAsync()
        {
            var now = GetFactoryNow();
            var today = now.Date;
            var todayRecords = await _repo.GetByDateAsync(today) ?? new List<Attendance>();
            var monthlyRecords = await _repo.GetMonthlyRecordsAsync(now.Month, now.Year) ?? new List<Attendance>();
            var workers = await _userRepo.GetAllWorkersAsync() ?? new List<User>();

            int workingDaysPassed = 0;
            for (int d = 1; d <= now.Day; d++)
            {
                var date = new DateTime(now.Year, now.Month, d);
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday) workingDaysPassed++;
            }

            var dailyAttendances = monthlyRecords.GroupBy(r => r.Date).Select(g => g.Select(x => x.UserId).Distinct().Count()).ToList();
            var avgAttendance = workingDaysPassed > 0 ? Math.Round((double)dailyAttendances.Sum() / workingDaysPassed, 1) : 0;
            var peakAttendance = dailyAttendances.Any() ? dailyAttendances.Max() : 0;

            var topPerformers = monthlyRecords.GroupBy(r => r.UserId)
                .Select(g => new { UserId = g.Key, Hours = g.Sum(r => r.TotalHours ?? 0) })
                .OrderByDescending(x => x.Hours).Take(2).ToList();

            var result = new SupervisorDashboardStatsDto
            {
                TotalPresent = todayRecords.Select(r => r.UserId).Distinct().Count(),
                ActiveWorkers = todayRecords.Count(r => r.SignOffTime == null),
                TotalHoursToday = Math.Round(todayRecords.Sum(r => r.TotalHours ?? 0), 2),
                MonthlyTotalHours = Math.Round(monthlyRecords.Sum(r => r.TotalHours ?? 0), 2),
                MonthlyAvgAttendance = avgAttendance,
                MonthlyPeakAttendance = peakAttendance
            };

            if (topPerformers.Count > 0)
            {
                result.TopPerformerName = workers.FirstOrDefault(w => w.Id == topPerformers[0].UserId)?.Name ?? "Unknown";
                result.TopPerformerHours = Math.Round(topPerformers[0].Hours, 2);
            }
            if (topPerformers.Count > 1)
            {
                result.RunnerUpName = workers.FirstOrDefault(w => w.Id == topPerformers[1].UserId)?.Name ?? "Unknown";
                result.RunnerUpHours = Math.Round(topPerformers[1].Hours, 2);
            }

            var latest = await _repo.GetByDateRangeAsync(today.AddDays(-7), today) ?? new List<Attendance>();
            result.RecentActivities = latest.OrderByDescending(x => x.SignInTime).SelectMany(r =>
            {
                var acts = new List<RecentActivityDto>();
                var name = workers.FirstOrDefault(w => w.Id == r.UserId)?.Name ?? "Unknown";
                if (r.SignInTime.HasValue) acts.Add(new RecentActivityDto { WorkerName = name, Action = "Sign In", Time = r.SignInTime.Value });
                if (r.SignOffTime.HasValue) acts.Add(new RecentActivityDto { WorkerName = name, Action = "Sign Off", Time = r.SignOffTime.Value });
                return acts;
            }).OrderByDescending(a => a.Time).Take(5).ToList();

            return result;
        }

        public async Task<SupervisorHomeDto?> GetSupervisorHomeSummaryAsync(DateTime? date = null)
        {
            var target = date ?? GetFactoryLocalDate();
            var records = await _repo.GetByDateAsync(target);
            if (records == null) return null;

            var workers = await _userRepo.GetAllWorkersAsync() ?? new List<User>();
            var inProgress = records.Count(r => r.SignOffTime == null);

            return new SupervisorHomeDto
            {
                TotalWorkers = workers.Count,
                CompletedShifts = records.Count(r => r.SignOffTime != null),
                InProgressShifts = inProgress,
                ActiveWorkers = inProgress
            };
        }

        public async Task<bool> ConfirmAutoSignOffAsync(int userId, DateTime date, DateTime actualSignOffTime)
        {
            var record = await _repo.GetPendingAutoSignOffAsync(userId, date.Date);
            if (record == null) return false;

            record.SignOffTime = actualSignOffTime;
            if (record.SignInTime.HasValue)
            {
                double totalMinutes = (record.SignOffTime.Value - record.SignInTime.Value).TotalMinutes;
                record.TotalHours = Math.Max(0, totalMinutes / 60.0);
            }
            record.Status = "SignedOff";
            return await _repo.UpdateAsync(record);
        }

        public async Task<bool> DeleteUserAttendanceAsync(int userId) => await _repo.DeleteByUserIdAsync(userId);

        public async Task<int> GetAttendanceCountAsync(int userId) => await _repo.GetUserHistoryCountAsync(userId);
    }
}
