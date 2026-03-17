using ShiftOne.Application.Dtos.Worker;

namespace ShiftOne.Application.Interfaces.Worker
{
    public interface IWorkerService
    {
        Task<WorkerHomeDto?> GetHomeAsync(int workerId);
    }
}
