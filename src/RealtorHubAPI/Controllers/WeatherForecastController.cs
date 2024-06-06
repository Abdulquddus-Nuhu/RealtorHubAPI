using Microsoft.AspNetCore.Mvc;
using RealtorHubAPI.Services;

namespace RealtorHubAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly MinioService _minioService;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, MinioService minioService)
        {
            _logger = logger;
            _minioService = minioService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }



        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly Dictionary<string, string> ContentTypeToExtension = new Dictionary<string, string>
        {
            { "video/mp4", ".mp4" },
            { "video/x-matroska", ".mkv" },
            { "video/avi", ".avi" },
            // Add other content types and their extensions as needed
        };

        [RequestSizeLimit(300_000_000)] // 300 MB in bytes
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, string url)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file selected.");
            }

            // Determine the file extension based on the content type
            var fileExtension = GetFileExtension(file.ContentType);

            if (string.IsNullOrEmpty(fileExtension))
            {
                return BadRequest("Unsupported file type.");
            }

            // Extract the filename and file URL
            var fileName = ExtractFileName(url);
            var fileUrl = ExtractFileUrl(url);

            // Append the appropriate extension to the filename
            var newFileName = $"{fileName}{fileExtension}";

            // Replace the old filename in the URL with the new filename
            var newUrl = ReplaceFileNameInUrl(url, fileName, newFileName);

            using (var stream = file.OpenReadStream())
            {
                // Create StreamContent with the correct Content-Type
                var content = new StreamContent(stream);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                var response = await httpClient.PutAsync(url, content);

             
                if (response.IsSuccessStatusCode)
                {
                    await _minioService.ReCopyFile(fileName ,newFileName);
                    return Ok(new { message = "File uploaded successfully!", newFileName, fileUrl });
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Failed to upload file.");
                }
            }
        }

        private string GetFileExtension(string contentType)
        {
            return ContentTypeToExtension.TryGetValue(contentType, out var extension) ? extension : null;
        }

        private string ReplaceFileNameInUrl(string url, string oldFileName, string newFileName)
        {
            return url.Replace(oldFileName, newFileName);
        }

        private string ExtractFileName(string url)
        {
            // Split the URL by '/' and get the last part, which is the filename with query parameters
            var parts = url.Split('/');
            var filenameWithQuery = parts[^1];

            // Split by '?' to remove the query parameters
            var filename = filenameWithQuery.Split('?')[0];
            return filename;
        }

        private string ExtractFileUrl(string url)
        {
            // Remove the protocol and host part of the URL
            var uri = new Uri(url);
            var pathAndQuery = uri.PathAndQuery;

            // Split by '?' to remove query parameters
            var path = pathAndQuery.Split('?')[0];

            return path;
        }
    }
}
