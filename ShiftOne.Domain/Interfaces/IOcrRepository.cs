using ShiftOne.Domain.Models;

namespace ShiftOne.Domain.Interfaces.Common {
    public interface IOcrRepository {
        Task<FormExtractResult?> ExtractAsync(Stream fileStream, string fileName, string contentType);
    }
}