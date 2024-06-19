using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.DataModel.Encryption;
using Minio.Exceptions;
using RealtorHubAPI.Configurations;
using RealtorHubAPI.Entities.Enums;
using RealtorHubAPI.Models.Responses;
using System.IO;
using System.IO.Pipes;
using System.Net.Mime;

namespace RealtorHubAPI.Services
{
    public class MinioService
    {

        private readonly IMinioClient _minioClient;
        private readonly MinioOptions _options;
        private readonly ILogger<MinioService> _logger;
        private readonly CloudinaryService _cloudinaryService;

        public MinioService(IOptions<MinioOptions> options, ILogger<MinioService> logger, CloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
            _logger = logger;
            _options = options.Value;
            _minioClient = new MinioClient()
                .WithEndpoint(_options.Endpoint)
                .WithCredentials(_options.AccessKey, _options.SecretKey)
                .WithSSL(_options.UseSSL)
                .Build();
        }

        public async Task<string> UploadFileAsync(IFormFile fileStream, FileType folder)
        {
            var stream = fileStream.OpenReadStream();
            string fileName = DateTime.UtcNow.Ticks.ToString();
            string filePath = $"{folder.ToString()}/";
            string fileExtension = Path.GetExtension(fileStream.FileName);
            string contentType = fileStream.ContentType;
            //string contentType = $"application/{fileExtension.Replace(".", string.Empty)}";
            string fileLocation = $"{_options.BucketName}/{filePath}{fileName}{fileExtension}";


            try
            {
                var args = new BucketExistsArgs()
                   .WithBucket(_options.BucketName);

                bool found = await _minioClient.BucketExistsAsync(args);
                _logger.LogInformation("{@mybucket}:bucket-name " + ((found == true) ? "exists" : "does not exist", _options.BucketName));

                if (!found)
                {
                    _logger.LogInformation("Creating bucket with name: {@name}", _options.BucketName);

                    var makeBucketArgs = new MakeBucketArgs()
                        .WithBucket(_options.BucketName);

                    await _minioClient.MakeBucketAsync(makeBucketArgs);
                    _logger.LogInformation("{@mybucket} is created successfully", _options.BucketName);
                }

                await _minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(filePath + fileName + fileExtension)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(contentType));
                _logger.LogInformation("Uploaded file successfully");

            }
            catch (MinioException e)
            {
                _logger.LogInformation("[Bucket]  Exception: {0}", e);
            }

            return fileLocation;
        }


        public async Task DownloadFileAsync(Stream memoryStream, string fileLocation)
        {
            var split = fileLocation.Split(new[] { _options.BucketName + "/" }, StringSplitOptions.None);

            if (split.Length < 2)
            {
                throw new ArgumentException("Invalid file location format", nameof(fileLocation));
            }

            var fileName = split[1];
      
            try
            {
                StatObjectArgs statObjectArgs = new StatObjectArgs()
                                                    .WithBucket(_options.BucketName)
                                                    .WithObject(fileName);
                await _minioClient.StatObjectAsync(statObjectArgs);

                GetObjectArgs getObjectArgs = new GetObjectArgs()
                                                  .WithBucket(_options.BucketName)
                                                  .WithObject(fileName)
                                                  .WithCallbackStream((stream) =>
                                                    {
                                                        stream.CopyTo(memoryStream);
                                                    });

                await _minioClient.GetObjectAsync(getObjectArgs);

            }
            catch (MinioException e)
            {
                _logger.LogInformation("Error occurred: {0}, while downloading file", e);
            }

        }


        public async Task<bool> DeleteFileAsync(string fileLocation)
        {
            var split = fileLocation.Split(_options.BucketName + "/");
            if (split.Length != 2) return false;

            var fileName = split[1];

            try
            {
                var bucketArgs = new BucketExistsArgs()
                    .WithBucket(_options.BucketName);

                if (!await _minioClient.BucketExistsAsync(bucketArgs)) return false;
                var removeArgs = new RemoveObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(fileName);
                await _minioClient.RemoveObjectAsync(removeArgs);

                return true;
            }
            catch (MinioException ex)
            {
                _logger.LogError(ex, "File Removal Error: {Message}", ex.Message);
                return false;
            }
        }

      
        public async Task<PresignedUrlResponse> GeneratePresignedUrlForUpload(int landId)
        {
            string newFileName = DateTime.UtcNow.Ticks.ToString();
            string filePath = $"{FileType.Videos.ToString()}/";
            //int expiry = 60 * 60 * 24; // 24 hours
            int expiry = 60 * 60 * 1; // 1 hours

            var presignedArgs = new PresignedPutObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(filePath + newFileName)
                    .WithExpiry(expiry);

            return new PresignedUrlResponse()
            {
                LandId = landId,
                FileName = newFileName,
                PresignedUrl = await _minioClient.PresignedPutObjectAsync(presignedArgs),
            };
        }


        public async Task<string> GeneratePresignedUrlForViewAsync(string fileLocation, int expiresInSeconds = 3600)
        {
            var split = fileLocation.Split(new[] { _options.BucketName + "/" }, StringSplitOptions.None);

            if (split.Length < 2)
            {
                throw new ArgumentException("Invalid file location format", nameof(fileLocation));
            }

            var fileName = split[1];

            try
            {
                bool exists = await DoesObjectExistAsync(fileName);
                if (!exists)
                {
                    return string.Empty;
                    //throw new FileNotFoundException($"The object {objectName} does not exist in the bucket {_bucketName}.");
                }

                var presignedArgs = new PresignedGetObjectArgs()
                         .WithBucket(_options.BucketName)
                         .WithObject(fileName)
                         .WithExpiry(expiresInSeconds);


                var presignedUrl = await _minioClient.PresignedGetObjectAsync(presignedArgs);
                return presignedUrl;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error in GeneratePresignedUrlAsync: {ex.Message}");
                return string.Empty;
            }
        }


        public async Task<bool> DoesObjectExistAsync(string objectName)
        {
            try
            {
                StatObjectArgs statObjectArgs = new StatObjectArgs()
                                             .WithBucket(_options.BucketName)
                                             .WithObject(objectName);

                //check if object exist
                var statObject = await _minioClient.StatObjectAsync(statObjectArgs);
                return statObject != null;

            }
            catch (MinioException ex) when (ex.Message.Contains("Object does not exist"))
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Error in DoesObjectExistAsync: {0}", ex.Message);
                return false;
                //throw;
            }
        }


        public async Task<bool> CompressAndUploadFile(string fileName)
        {
            _logger.LogInformation($"Starting compression for file: {fileName}");

            try
            {
                var split = fileName.Split(new[] { _options.BucketName + "/" }, StringSplitOptions.None);
                if (split.Length < 2)
                {
                    _logger.LogError("Invalid file location format for fileName: {fileName}", fileName);
                    throw new ArgumentException("Invalid file location format", nameof(fileName));
                }

                var cloudnaryFileName = split[1];
                _logger.LogInformation($"Extracted cloudinary file name: {cloudnaryFileName}");

                var exist = await DoesObjectExistAsync(cloudnaryFileName);
                if (!exist)
                {
                    _logger.LogInformation("Object doesn't exist: {cloudnaryFileName}", cloudnaryFileName);
                    return false;
                }

                // Download large file from MinIO
                _logger.LogInformation("Downloading large file: {cloudnaryFileName}, from MinIO server", cloudnaryFileName);
                var memoryStream = new MemoryStream();
                await DownloadFileAsync(memoryStream, fileName);

                // Upload to Cloudinary for compression
                _logger.LogInformation("Uploading to Cloudinary for compression: {cloudnaryFileName}", cloudnaryFileName);
                var compressedUrl = await _cloudinaryService.CompressVideoAsync(memoryStream, cloudnaryFileName);

                // Download compressed file
                using (var client = new HttpClient())
                using (var response = await client.GetAsync(compressedUrl.SecureUrl))
                {
                    response.EnsureSuccessStatusCode();
                    var contentStream = await response.Content.ReadAsStreamAsync();

                    await _minioClient.PutObjectAsync(new PutObjectArgs()
                        .WithBucket(_options.BucketName)
                        .WithObject(cloudnaryFileName)
                        .WithStreamData(contentStream)
                        .WithObjectSize(contentStream.Length));
                    _logger.LogInformation("Uploaded file {cloudnaryFileName} successfully", cloudnaryFileName);
                }

                _logger.LogInformation("Deleting file with publicId: {publicId}, from Cloudinary",compressedUrl.PublicId);
                await _cloudinaryService.DeleteFileAsync(compressedUrl.PublicId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in Compressing And UploadFile: {errorMessage}", ex.Message);
                return false;
            }
        }


        public async Task <bool> ReCopyFile(string oldFileName, string newFileName)
        {
            try
            {
                var copyARgs = new CopySourceObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject($"Videos/{oldFileName}");

                // Copy the object to a new name with the .mp4 extension
                await _minioClient.CopyObjectAsync(new CopyObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject($"Videos/{newFileName}")
                .WithCopyObjectSource(copyARgs)
                );

                // Remove the original object
                await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject($"Videos/{oldFileName}"));

                _logger.LogInformation($"Successfully renamed '{oldFileName}' to '{newFileName}'");
                return true;
            }
            catch (MinioException e)
            {
                _logger.LogInformation($"[MinioException] {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"[Exception] {e.Message}");
                return false;
            }

        }


        public string GetFileUrl(string objectName)
        {
            return $"{(_options.UseSSL ? "https" : "http")}://{_options.Endpoint}/{_options.BucketName}/{objectName}";
        }


    }
}
