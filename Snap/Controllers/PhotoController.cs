using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Snap.Areas.Identity.Data.Data;
using Snap.Helpers;
using Snap.Models;
using Snap.Services;

namespace Snap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoController : ControllerBase
    {
        private readonly IFinalImageService _finalImageService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PhotoController(IFinalImageService finalImageService, UserManager<ApplicationUser> userManager)
        {
            _finalImageService = finalImageService;
            _userManager = userManager;
        }

        // POST api/photo/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto([FromForm] PhotoUploadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SessionId) || request.File == null)
                return BadRequest(new { message = "SessionId and photo file are required." });

            try
            {
                var (expectedRows, expectedCols) = LayoutPresets.GetGrid(request.LayoutType);
                var tempSessionFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "temp", request.SessionId);
                if (!Directory.Exists(tempSessionFolder))
                    Directory.CreateDirectory(tempSessionFolder);

                var timestamp = DateTime.Now;
                var fileName = $"{request.SessionId}_{request.Sequence}_{timestamp:yyyyMMdd_HHmmss}.png";
                var filePath = Path.Combine(tempSessionFolder, fileName);
                var (targetWidth, targetHeight) = LayoutPresets.GetPhotoSize(request.LayoutType);
                using var image = Image.Load<Rgba32>(request.File.OpenReadStream());
                image.Mutate(ctx => ctx.Resize(250, 180));

                await image.SaveAsPngAsync(filePath);

                var (width, height) = LayoutPresets.GetPhotoSize(request.LayoutType);
                var photo = new CapturedPhoto
                {
                    FileName = fileName,
                    CapturedAt = timestamp,
                    SessionId = request.SessionId,
                    Sequence = request.Sequence,
                    LayoutType = request.LayoutType,
                    Width = width,
                    Height = height
                };

                return Ok(new
                {
                    fileName,
                    sessionId = request.SessionId,
                    sequence = request.Sequence,
                    capturedAt = timestamp,
                    width,
                    height
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Upload error: {ex.Message}");
            }
        }

        // POST api/photo/compile
        [HttpPost("compile")]
        public async Task<IActionResult> CompileAndEdit([FromBody] EditPhotoRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.SessionId) || request.LayoutType <= 0)
                    return BadRequest("SessionId and LayoutType are required.");

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var (rows, cols) = LayoutPresets.GetGrid(request.LayoutType);
                int expectedCount = rows * cols;
                var tempSessionFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "temp", request.SessionId);
                if (!Directory.Exists(tempSessionFolder))
                    return BadRequest("No photos uploaded for this session.");

                var allFiles = Directory.GetFiles(tempSessionFolder, "*.png");
                var images = allFiles
                    .Where(f => Path.GetFileName(f).StartsWith(request.SessionId))
                    .OrderBy(f =>
                    {
                        var parts = Path.GetFileNameWithoutExtension(f).Split('_');
                        return int.TryParse(parts.ElementAtOrDefault(1), out var seq) ? seq : 9999;
                    })
                    .ToList();

                int retries = 10;
                while (images.Count < expectedCount && retries-- > 0)
                {
                    await Task.Delay(200);
                    images = Directory.GetFiles(tempSessionFolder, "*.png")
                        .Where(f => Path.GetFileName(f).StartsWith(request.SessionId))
                        .OrderBy(f =>
                        {
                            var parts = Path.GetFileNameWithoutExtension(f).Split('_');
                            return int.TryParse(parts.ElementAtOrDefault(1), out var seq) ? seq : 9999;
                        })
                        .ToList();
                }

                if (images.Count < expectedCount)
                    return BadRequest($"Expected {expectedCount} photos but found {images.Count}. Please upload all photos before compiling.");

                var finalRequest = new FinalImageRequest
                {
                    SessionId = request.SessionId,
                    FilterId = request.FilterId,
                    StickerId = request.StickerId,
                    FrameColor = request.FrameColor,
                    IncludeTimestamp = request.IncludeTimestamp,
                    LayoutType = request.LayoutType
                };

                var imagePath = await _finalImageService.GenerateFinalImageAsync(finalRequest, user.Id);

                return Ok(new { message = "Final image generated successfully.", imagePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error compiling and editing photos: {ex.Message}");
            }
        }

        // GET api/photo/download/{sessionId}
        [HttpGet("download/{sessionId}")]
        public IActionResult DownloadFinalImage(string sessionId)
        {
            var finalFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "final");
            var file = Directory.GetFiles(finalFolder, $"{sessionId}_*.png")
                .OrderByDescending(f => new FileInfo(f).CreationTime)
                .FirstOrDefault();

            if (file == null)
                return NotFound(new { message = "No final image found for this session." });

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(file, out var contentType))
                contentType = "application/octet-stream";

            var bytes = System.IO.File.ReadAllBytes(file);
            var fileName = Path.GetFileName(file);

            return File(bytes, contentType, fileName);
        }

        // GET api/photo/list/{sessionId}
        [HttpGet("list/{sessionId}")]
        public IActionResult ListSessionPhotos(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return BadRequest("SessionId is required.");

            var tempSessionFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "temp", sessionId);
            if (!Directory.Exists(tempSessionFolder))
                return NotFound("No photos uploaded for this session.");

            var allFiles = Directory.GetFiles(tempSessionFolder, "*.png");
            var images = allFiles
                .Where(f => Path.GetFileName(f).StartsWith(sessionId))
                .OrderBy(f =>
                {
                    var parts = Path.GetFileNameWithoutExtension(f).Split('_');
                    return int.TryParse(parts.ElementAtOrDefault(1), out var seq) ? seq : 9999;
                })
                .Select(f => Path.GetFileName(f))
                .ToList();

            return Ok(new
            {
                SessionId = sessionId,
                PhotoCount = images.Count,
                Photos = images
            });
        }
    }
}
