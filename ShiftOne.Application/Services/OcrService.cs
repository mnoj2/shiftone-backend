using ShiftOne.Application.Dtos;
using ShiftOne.Application.Interfaces;
using ShiftOne.Domain.Interfaces.Common;

namespace ShiftOne.Application.Services.Admin {
    public class OcrService : IOcrService {
        private readonly IOcrRepository _ocrRepository;

        public OcrService(IOcrRepository ocrRepository) {
            _ocrRepository = ocrRepository;
        }

        public async Task<FormExtractDto?> ExtractAsync(Stream fileStream, string fileName, string contentType) {
            var result = await _ocrRepository.ExtractAsync(fileStream, fileName, contentType);
            if(result == null)
                return null;

            return new FormExtractDto {
                Name = result.Name,
                Email = result.Email,
                Phone = result.Phone,
                Role = result.Role
            };
        }

    }
}