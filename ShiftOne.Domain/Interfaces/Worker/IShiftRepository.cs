using ShiftOne.Domain.Entities;

namespace ShiftOne.Domain.Interfaces.Worker
{
    public interface IShiftRepository
    {
        Task<Attendance?> GetShiftByWorkerAndDateAsync(int workerId, DateTime date);
    }
}
