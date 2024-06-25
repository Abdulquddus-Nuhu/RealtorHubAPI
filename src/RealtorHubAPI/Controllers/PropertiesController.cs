using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RealtorHubAPI.Data;
using RealtorHubAPI.Entities;
using RealtorHubAPI.Entities.Enums;
using RealtorHubAPI.Models.Requests;
using RealtorHubAPI.Models.Responses;
using RealtorHubAPI.Services;
using RealtorHubAPI.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using System.Text.Json;
using Property = RealtorHubAPI.Entities.Property;

namespace RealtorHubAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    //[Route("api/[controller]")]
    [Route("api/property")]
    [ApiController]
    public class PropertiesController : BaseController
    {
        private readonly MinioService _minioService;
        private readonly AppDbContext _context;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<PropertiesController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PropertiesController(MinioService minioService, AppDbContext context
            , IBackgroundTaskQueue taskQueue, ILogger<PropertiesController> logger, IServiceProvider serviceProvider)
        {
            _minioService = minioService;
            _context = context;
            _taskQueue = taskQueue;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
          Summary = "Create A New Property Endpoint",
          Description = "It requires Admin or Realtor privelage")
         //OperationId = "auth.login",
         //Tags = new[] { "AuthEndpoints" })
         ]
        [HttpPost("create-property")]
        public async Task<IActionResult> CreateProperty(CreateLandRequest request)
        {
            var response = new BaseResponse();

            var propertyExist = await _context.Properties.AnyAsync(x => x.Title == request.Title);
            if (propertyExist)
            {
                response.Status = false;
                response.Message = "A Property with a similar title already exist";
                response.Code = StatusCodes.Status400BadRequest;
                return BadRequest(response);
            }

            var property = new Property()
            {
                Location = request.Location,
                Description = request.Description,
                Title = request.Title,
                Area = request.Area,
                Price = request.Price,
                Type = request.Type,
                UserId = UserId
            };

            await _context.AddAsync(property);
            await _context.SaveChangesAsync();

            return Ok(new { propertyId = property.Id, Messsage = "Property created successfully" });
        }

        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
          Summary = "Get A Property Endpoint",
          Description = "")
         //OperationId = "auth.login",
         //Tags = new[] { "AuthEndpoints" })
         ]
        [HttpGet("{propertyId}")]
        public async Task<IActionResult> GetProperty(int propertyId)
        {
            var property = await _context.Properties
                .Include(l => l.Images)
                .Include(l => l.Videos)
                .Select(l => new PropertyResponse()
                {
                    PropertyId = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    IsAvailable = l.IsAvailable,
                    Location = l.Location,
                    Area = l.Area,
                    Price = l.Price,
                    PropertyType = l.Type.ToString(),
                    UserId = l.UserId,
                    Images = l.Images.Select(c => new PropertyFileResponse { FileId = c.Id, Url = c.Url, }),
                    Videos = l.Videos.Select(c => new PropertyFileResponse { FileId = c.Id, Url = c.Url, }),
                })
                .SingleOrDefaultAsync(l => l.PropertyId == propertyId);


            if (property == null) return NotFound(new BaseResponse() { Code = 404, Message = "Property Not Found", Status = false });


            return Ok(property);
        }

        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
          Summary = "Delete A Property Endpoint",
          Description = "It requires Admin or Realtor privelage")
         //OperationId = "auth.login",
         //Tags = new[] { "AuthEndpoints" })
         ]
        [HttpDelete("{propertyId}")]
        public async Task<IActionResult> DeleteProperty(int propertyId)
        {
            var property = await _context.Properties.FirstOrDefaultAsync(l => l.Id == propertyId);

            if (property == null) return NotFound(new BaseResponse() { Code = 404, Message = "Property Not Found", Status = false });

            _context.Remove(property);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse() { Code = 200, Message = "Property Deleted", Status = true });
        }

        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
          Summary = "Get All Properties Endpoint",
          Description = "")
         //OperationId = "auth.login",
         //Tags = new[] { "AuthEndpoints" })
         ]
        [HttpGet("list")]
        public async Task<IActionResult> GetProperties()
        {
            var isRealtor = User.IsInRole(nameof(RoleType.Realtor));

            var lands = await _context.Properties
                .Include(l => l.Images)
                .Include(l => l.Videos)
                .Select(l => new PropertyResponse()
                {
                    PropertyId = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    IsAvailable = l.IsAvailable,
                    Location = l.Location,
                    Area = l.Area,
                    Price = l.Price,
                    PropertyType = l.Type.ToString(),
                    UserId = l.UserId,
                    Images = l.Images.Select(c => new PropertyFileResponse { FileId = c.Id, Url = c.Url, }),
                    Videos = l.Videos.Select(c => new PropertyFileResponse { FileId = c.Id, Url = c.Url, }),
                })
                .ToListAsync();

            return Ok(lands);
        }

        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
          Summary = "Upload Image For A New Property Endpoint",
          Description = "It requires Admin or Realtor privelage")
         //OperationId = "auth.login",
         //Tags = new[] { "AuthEndpoints" })
         ]
        [HttpPost("{propertyId}/image")]
        public async Task<IActionResult> UploadImage(int propertyId, IFormFile file)
        {
            var property = _context.Properties.FirstOrDefault(l => l.Id == propertyId);
            if (property == null)
            {
                return NotFound(new BaseResponse() { Code = 404, Message = "Property not found", Status = false });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new BaseResponse() { Message = "File not selected", Status = false });
            }

            var fileLocation = await _minioService.UploadFileAsync(file, FileType.Images);
            if (string.IsNullOrWhiteSpace(fileLocation))
            {
                return StatusCode(500, new BaseResponse() { Message = "File not uploaded successfully! Please try again", Status = false });
            }

            var propertyImage = new PropertyImage()
            {
                PropertyId = propertyId,
                Url = fileLocation
            };

            await _context.AddAsync(propertyImage);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Image File uploaded and processing started." });
        }

        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
          Summary = "Generate Presigned URL To Be Able Upload Video Files That Are More Than 30MB Endpoint",
          Description = "")
         //OperationId = "auth.login",
         //Tags = new[] { "AuthEndpoints" })
         ]
        [HttpPost("generate-presigned-url")]
        public async Task<IActionResult> GeneratePresignedUrl(int propertyId)
        {
            var presignedUrl = await _minioService.GeneratePresignedUrlForUpload(propertyId);
            return Ok(presignedUrl);
        }

        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
          Summary = "Notify The Server Of A Heavy File More Than 30MB Upload Endpoint",
          Description = "")
         //OperationId = "auth.login",
         //Tags = new[] { "AuthEndpoints" })
         ]
        [HttpPost("notify-file-upload")]
        public IActionResult NotifyFileUpload([FromBody] FileNotificationRequest request)
        {
            // Log the received fileName for debugging purposes
            _logger.LogInformation($"Received fileName: {request.FileUrl}");

            //Enqueue the compression job
            _taskQueue.QueueBackgroundWorkItem(async token =>
            {
                _logger.LogInformation("Background task started for fileName: {fileName}", request.FileUrl);
                bool compressedFileUrl = await _minioService.CompressAndUploadFile(request.FileUrl);

                if (compressedFileUrl)
                {
                    _logger.LogInformation("File compressed and uploaded successfully: {fileName}", request.FileUrl);
                }
                else
                {
                    _logger.LogError("Failed to compress and upload file: {fileName}", request.FileUrl);
                }
            });

            return Ok(new { message = "File upload notification received and compression job enqueued" });
        }


        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
          Summary = "Upload Video For A Property Endpoint",
          Description = "It requires Admin or Realtor privelage")
         //OperationId = "auth.login",
         //Tags = new[] { "AuthEndpoints" })
         ]
        [HttpPost("{propertyId}/video")]
        //[RequestSizeLimit(104857600)] // 100 MB
        public async Task<IActionResult> UploadVideo(int propertyId, IFormFile file)
        {
            var property = _context.Properties.FirstOrDefault(l => l.Id == propertyId);
            if (property == null)
            {
                return NotFound(new BaseResponse() { Code = 404, Message = "Property not found", Status = false });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new BaseResponse() { Message = "File not selected", Status = false });
            }

            var fileLocation = await _minioService.UploadFileAsync(file, FileType.Videos);
            if (string.IsNullOrWhiteSpace(fileLocation))
            {
                return StatusCode(500, new BaseResponse() { Message = "File not uploaded successfully! Please try again", Status = false });
            }

            var propertyVideo = new PropertyVideo()
            {
                PropertyId = propertyId,
                Url = fileLocation
            };

            await _context.AddAsync(propertyVideo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Video File uploaded and processing started." });
        }

        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
          Summary = "Generate Presigned URL To Be Able To View A Video or Image Endpoint",
          Description = "")
         //OperationId = "auth.login",
         //Tags = new[] { "AuthEndpoints" })
         ]
        [HttpGet("generate-presigned-url")]
        public async Task<IActionResult> GeneratePresignedUrlForView([FromQuery] string fileName)
        {
            try
            {
                var url = await _minioService.GeneratePresignedUrlForViewAsync(fileName, 3600); // URL valid for 1 hour
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("delete-file")]
        public async Task<IActionResult> DeleteFile(string fileLocation)
        {
            await _minioService.DeleteFileAsync(fileLocation);


            //Enqueue the compression job
            //_taskQueue.QueueBackgroundWorkItem(async token =>
            //{
            //    await using var scope = _serviceProvider.CreateAsyncScope();
            //    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            //    _logger.LogInformation("Deleting assocaited record from database of file deleted from Minio for fileLocation: {fileLocation}", fileLocation);

            //    var split = fileLocation.Split("/");

            //    var locationType = split[1];
            //    if (locationType == FileType.Images.ToString())
            //    {

            //        var image = await context.LandImages.FirstOrDefaultAsync(x => x.Url == fileLocation);
            //        if (image is not null)
            //        {
            //            _logger.LogInformation("Property image deleted \n {data}", JsonSerializer.Serialize(image));
            //            _context.Remove(image);
            //        }
            //    }
                
            //    if (locationType == FileType.Videos.ToString())
            //    {
            //        var video = await context.LandVideos.FirstOrDefaultAsync(x => x.Url == fileLocation);
            //        if (video is not null)
            //        {
            //            _logger.LogInformation("Property video deleted \n {data}", JsonSerializer.Serialize(video));
            //            _context.Remove(video);
            //        }
            //    }

            //});


            return Ok(new { message = "File deleted successfully." });
        }

        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
          Summary = "Download A File Endpoint",
          Description = "")
         //OperationId = "auth.login",
         //Tags = new[] { "AuthEndpoints" })
         ]
        [HttpGet("download-file")]
        public async Task<ActionResult> DownloadFileAsync(string fileLocation)
        {
            try
            {
                var ext = Path.GetExtension(fileLocation).ToLower();
                var contentType = MimeTypesHelper.GetContentType(ext);

                var memoryStream = new MemoryStream();
                await _minioService.DownloadFileAsync(memoryStream, fileLocation);
                memoryStream.Seek(0, SeekOrigin.Begin); // Reset the stream position

                return new FileStreamResult(memoryStream, contentType)
                {
                    FileDownloadName = Path.GetFileName(fileLocation)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("File Download Exception : {0}", ex.Message);
                return StatusCode(500, new { Message = "An error occurred, please try again." });
            }
        }
    }
}
