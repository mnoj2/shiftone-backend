using ShiftOne.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShiftOne.Application.Interfaces {
    public interface IOcrService {
        Task<FormExtractDto?> ExtractFormDataAsync(Stream fileStream, string fileName, string contentType);

    }
}
