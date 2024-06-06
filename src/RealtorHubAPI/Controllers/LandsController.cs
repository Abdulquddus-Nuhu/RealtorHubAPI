using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtorHubAPI.Data;
using RealtorHubAPI.Entities;
using RealtorHubAPI.Entities.Enums;
using RealtorHubAPI.Models.Requests;
using RealtorHubAPI.Models.Responses;
using RealtorHubAPI.Services;
using RealtorHubAPI.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace RealtorHubAPI.Controllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class LandsController : BaseController
    {
        private readonly MinioService _minioService;
        private readonly AppDbContext _context;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<LandsController> _logger;

        public LandsController(MinioService minioService, AppDbContext context
            , IBackgroundTaskQueue taskQueue, ILogger<LandsController> logger)
        {
            _minioService = minioService;
            _context = context;
            _taskQueue = taskQueue;
            _logger = logger;
        }

        [HttpPost("create-land")]
        public async Task<IActionResult> CreateLand(CreateLandRequest request)
        {
            var response = new BaseResponse();

            var landExist = await _context.Lands.AnyAsync(x => x.Title == request.Title);
            if (landExist)
            {
                response.Status = false;
                response.Message = "A land with a similar title already exist";
                response.Code = StatusCodes.Status400BadRequest;
                return BadRequest(response);
            }

            var land = new Land()
            {
                Id = Guid.NewGuid(),
                Location = request.Location,
                Description = request.Description,
                Title = request.Title,
                UserId = UserId
            };

            await _context.AddAsync(land);
            await _context.SaveChangesAsync();
            return Ok(new { LandId = land.Id, Messsage = "Land created successfully"});
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetLand(Guid id)
        {
            var land = await _context.Lands
                .Include(l => l.Images)
                .Include(l => l.Videos)
                .SingleOrDefaultAsync(l => l.Id == id);

            if (land == null) return NotFound();

            return Ok(land);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLand(Guid id)
        {
            var land = await _context.Lands.FirstOrDefaultAsync(l => l.Id == id);
            if (land == null)
            {
                return NotFound();
            }

            _context.Remove(land);
            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpGet("list")]
        public async Task<IActionResult> GetLands()
        {
            var isRealtor = User.IsInRole(nameof(RoleType.Realtor));

            var lands = await _context.Lands
                .Include(l => l.Images)
                .Include(l => l.Videos)
                .ToListAsync();

            return Ok(lands);
        }


        [HttpPost("{landId}/image")]
        public async Task<IActionResult> UploadImage(Guid landId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new BaseResponse() { Message = "File not selected", Status = false });
            }

            var fileLocation = await _minioService.UploadFileAsync(file, FileType.Images);
            //todo: save filelocation to land with landid

            return Ok(new { message = "Image File uploaded and processing started." });
        }

        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
          Summary = "Generate Presigned URL To Be Able Upload Video Files That Are More Than 50MB",
          Description = "")
         //OperationId = "auth.login",
         //Tags = new[] { "AuthEndpoints" })
         ]
        [HttpPost("generate-presigned-url")]
        public async Task<IActionResult> GeneratePresignedUrl(Guid landId)
        {
            var presignedUrl = await _minioService.GeneratePresignedUrlForUpload(landId);
            return Ok(presignedUrl);
        }


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



        [HttpPost("{landId}/video")]
        //[RequestSizeLimit(104857600)] // 100 MB
        public async Task<IActionResult> UploadVideo(Guid landId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new BaseResponse() { Message = "File not selected", Status = false });
            }

            var fileLocation = await _minioService.UploadFileAsync(file, FileType.Videos);
            //todo: save filelocation to land with landid
            return Ok(new { message = "Video File uploaded and processing started." });
        }

        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
          Summary = "Generate Presigned URL To Be Able To View A Video or Image",
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
            return Ok(new { message = "File deleted successfully." });
        }

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
