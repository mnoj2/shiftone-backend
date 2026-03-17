using Microsoft.Extensions.Configuration;
using ShiftOne.Application.Dtos;
using ShiftOne.Application.Interfaces;
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

        private DateTime GetFactoryNow() {
            var tzId = _config.GetValue<string>("FactoryLocation:TimeZone") ?? "India Standard Time";
            var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }

        private DateTime GetFactoryLocalDate() => GetFactoryNow().Date;

        public async Task<string?> SignInAsync(int userId) {

            var today = GetFactoryLocalDate();
            if (await _repo.HasAttendanceAsync(userId, today)) return null;

            var record = new Attendance {
                UserId = userId,
                Date = today,
                SignInTime = DateTime.UtcNow,
                Status = "SignedIn"
            };

            return await _repo.AddAsync(record) ? "Signed In Successfully" : null;
        }

        public async Task<string?> SignOffAsync(int userId) {

            var today = GetFactoryLocalDate();
            var record = await _repo.GetActiveShiftAsync(userId, today);
            if (record == null) return null;

            record.SignOffTime = DateTime.UtcNow;

            if (record.SignInTime.HasValue) {
                double totalMinutes = (record.SignOffTime.Value - record.SignInTime.Value).TotalMinutes;
                record.TotalHours = Math.Max(0, totalMinutes / 60.0);
            }
            record.Status = "SignedOff";

            return await _repo.UpdateAsync(record) ? "Signed Off Successfully" : null;
        }

        public async Task<AttendanceInfoDto?> GetTodayInfoAsync(int userId) {
            var today = GetFactoryLocalDate();
            var record = await _repo.GetTodayRecordAsync(userId, today);

            if (record == null) { 
                return new AttendanceInfoDto {
                    Status = "Not Started"
                };
            }

            double? displayHours = record.TotalHours;
            if (record.Status == "SignedIn" && record.SignInTime.HasValue) {
                var now = DateTime.UtcNow;
                double rawMinutes = (now - record.SignInTime.Value).TotalMinutes;
                displayHours = Math.Max(0, rawMinutes / 60.0);
            }

            return new AttendanceInfoDto {
                Status = record.Status,
                SignInTime = record.SignInTime,
                SignOffTime = record.SignOffTime,
                TotalHours = displayHours
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

        public async Task<List<AttendanceRecordDto>?> GetWorkerHistoryAsync(int userId) {
            var data = await _repo.GetUserHistoryAsync(userId);
            if (data == null) return null;

            var worker = await _userRepo.GetByIdAsync(userId);
            var name = worker?.Name ?? "Unknown";

            return data.Select(r => new AttendanceRecordDto {
                Date = r.Date,
                UserId = r.UserId,
                WorkerName = name,
                SignInTime = r.SignInTime,
                SignOffTime = r.SignOffTime,
                Status = r.Status,
                TotalHours = r.TotalHours
            }).ToList();
        }

        public async Task<List<AttendanceRecordDto>?> GetByDateRangeAsync(DateTime start, DateTime end) {
            var data = await _repo.GetByDateRangeAsync(start, end);
            if (data == null) return null;

            var workers = await _userRepo.GetAllWorkersAsync();
            return data.Select(r => new AttendanceRecordDto {
                Date = r.Date,
                UserId = r.UserId,
                WorkerName = workers?.FirstOrDefault(w => w.Id == r.UserId)?.Name ?? "Unknown",
                SignInTime = r.SignInTime,
                SignOffTime = r.SignOffTime,
                Status = r.Status,
                TotalHours = r.TotalHours
            }).ToList();
        }

        public async Task<SupervisorAnalyticsDto?> GetSupervisorAnalyticsAsync(int month, int year)
        {
            var records = await _repo.GetMonthlyRecordsAsync(month, year);
            if (records == null) return null;

            var monthlyHours = records.GroupBy(r => r.Date).Select(g => new DailyHoursDto
            {
                Date = g.Key.ToString("dd MMM"),
                Hours = Math.Round(g.Sum(r => r.TotalHours ?? 0), 2)
            }).OrderBy(x => x.Date).ToList();

            return new SupervisorAnalyticsDto
            {
                MonthlyHours = monthlyHours
            };
        }

        public async Task<SupervisorHomeDto?> GetSupervisorHomeSummaryAsync(DateTime? date = null) {
            var target = date ?? GetFactoryLocalDate();
            var records = await _repo.GetByDateAsync(target);
            if (records == null) return null;

            var workers = await _userRepo.GetAllWorkersAsync() ?? new List<User>();

            return new SupervisorHomeDto {
                TotalWorkers = workers.Count,
                CompletedShifts = records.Count(r => r.SignOffTime != null),
                InProgressShifts = records.Count(r => r.SignOffTime == null)
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

        public async Task<bool> ManualSignOffAsync(int userId, DateTime date, DateTime signOffTime) {
            var record = await _repo.GetActiveShiftAsync(userId, date.Date);
            if (record == null || record.Status != "SignedIn") return false;

            record.SignOffTime = signOffTime;

            if (record.SignInTime.HasValue) {
                double totalMinutes = (record.SignOffTime.Value - record.SignInTime.Value).TotalMinutes;
                record.TotalHours = Math.Max(0, totalMinutes / 60.0);
            }

            record.Status = "SignedOff";

            return await _repo.UpdateAsync(record);
        }

        public async Task<bool> DeleteUserAttendanceAsync(int userId) => await _repo.DeleteByUserIdAsync(userId);
    }
}
