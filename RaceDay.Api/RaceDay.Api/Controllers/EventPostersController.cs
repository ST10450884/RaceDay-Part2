
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Services;
using RaceDay.Api.Authorization;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventPostersController : ControllerBase
    {
        private readonly BlobStorageService _blobStorage;

        private readonly string[] _allowedTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        public EventPostersController(BlobStorageService blobStorage)
        {
            _blobStorage = blobStorage;
        }

        [HttpPost("upload")]
        [SessionAuthorize("Organiser")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Please select a poster." });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Maximum poster size is 5 MB." });

            if (!_allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
                return BadRequest(new { message = "Only JPG, PNG and WEBP images are allowed." });

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (extension != ".jpg" && extension != ".jpeg" &&
                extension != ".png" && extension != ".webp")
                return BadRequest(new { message = "Invalid image extension." });

            await using var stream = file.OpenReadStream();

            string blobName = await _blobStorage.UploadAsync(
                stream,
                file.FileName,
                file.ContentType);

            return Ok(new
            {
                posterUrl = $"/api/EventPosters/{Uri.EscapeDataString(blobName)}"
            });
        }

        [HttpGet("{blobName}")]
        public async Task<IActionResult> GetPoster(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName) ||
                blobName.Contains('/') ||
                blobName.Contains('\\'))
                return BadRequest();

            var stream = await _blobStorage.DownloadAsync(blobName);

            if (stream == null)
                return NotFound();

            string extension = Path.GetExtension(blobName).ToLowerInvariant();

            string contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            return File(stream, contentType);
        }

        [HttpDelete("{blobName}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> DeletePoster(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName) ||
                blobName.Contains('/') ||
                blobName.Contains('\\'))
                return BadRequest();

            await _blobStorage.DeleteAsync(blobName);

            return Ok(new { message = "Poster deleted successfully." });
        }
    }
}
