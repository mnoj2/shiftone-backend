using ShiftOne.Application.Dtos;

namespace ShiftOne.Application.Interfaces {
    public interface IOcrService {
        Task<FormExtractDto?> ExtractFormDataAsync(Stream fileStream, string fileName, string contentType);

    }
}
