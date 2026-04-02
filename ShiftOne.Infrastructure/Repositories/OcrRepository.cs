using Microsoft.Extensions.Configuration;
using ShiftOne.Domain.Interfaces.Common;
using ShiftOne.Domain.Models;
using System.Text.Json;

namespace ShiftOne.Infrastructure.Repositories.Common {
    public class OcrRepository : IOcrRepository {
        private readonly HttpClient _client;
        private readonly string OcrUrl;

        public OcrRepository(HttpClient client, IConfiguration configuration) {
            _client = client;
            OcrUrl = configuration["OcrUrl"] ?? throw new InvalidOperationException("OCR service URL not configured");
        }

        // Sends the file to the OCR service and returns the extracted form data
        public async Task<FormExtractResult?> ExtractAsync(Stream fileStream, string fileName, string contentType) {

            using(var form = new MultipartFormDataContent()) {
                using(var fileContent = new StreamContent(fileStream)) {

                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    form.Add(fileContent, "file", fileName);

                    var response = await _client.PostAsync(OcrUrl, form);

                    if(!response.IsSuccessStatusCode)
                        return null;

                    var json = await response.Content.ReadAsStringAsync();

                    return JsonSerializer.Deserialize<FormExtractResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
        }
    }
}