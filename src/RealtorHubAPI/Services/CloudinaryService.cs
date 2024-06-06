using Microsoft.Extensions.Options;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using RealtorHubAPI.Configurations;
using System.Net;

namespace RealtorHubAPI.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IOptions<CloudinaryOptions> options, ILogger<CloudinaryService> logger)
        {
            var account = new Account(
                options.Value.CloudName,
                options.Value.ApiKey,
                options.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
            _logger = logger;
        }

        public async Task<(string SecureUrl, string PublicId)> CompressVideoAsync(MemoryStream file, string cloudnaryFileName)
        {
            // Ensure the MemoryStream position is set to the beginning
            file.Position = 0;

            var uploadParams = new VideoUploadParams()
            {
                File = new FileDescription(cloudnaryFileName, file),
                Transformation = new Transformation()
                        .BitRate("500k") // Adjust the bitrate
                        .Width("auto") // Keep the same width
                        .Quality("auto")
                        .FetchFormat("auto")
                        .Crop("limit")
                        //.Width(1280)
                        //.Height(720)
            };

            _logger.LogInformation("Uploading video with name: {name}, to Cloudinary", cloudnaryFileName);

            // Perform the upload
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // Check if upload was successful
            if (uploadResult.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Upload successfull with url: {URL}", uploadResult.SecureUrl);
                return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId);
            }
            else
            {
                _logger.LogError("Upload failed: {Message}", uploadResult.Error.Message);
                // Handle error (you can throw an exception or return an error message)
                throw new Exception($"Upload failed: {uploadResult.Error.Message}");
            }
        }

        public async Task DeleteFileAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Video
            };

            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            if (deletionResult.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogInformation("Deletion failed for publicId: {publicId}, with error: { deletionResult.Error.Message}", publicId, deletionResult.Error.Message);
                //throw new Exception($"Deletion failed: {deletionResult.Error.Message}");
            }
        }

    }
}
