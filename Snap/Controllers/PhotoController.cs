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
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Snap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
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
                //int layoutTypeNum = request.LayoutType;                // Assumes it's "2", "3", "4"
                //var (expectedRows, expectedCols) = LayoutPresets.GetGrid(layoutTypeNum);

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
                //if (string.IsNullOrWhiteSpace(request.SessionId) || string.IsNullOrWhiteSpace(request.LayoutType))
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

                var images = Directory.GetFiles(tempSessionFolder, "*.png");
                if (images.Length < expectedCount)
                    return BadRequest($"Expected {expectedCount} photos but found {images.Length}. Please upload all photos before compiling.");

                var finalRequest = new FinalImageRequest
                {
                    SessionId = request.SessionId,
                    FilterId = request.FilterId,
                    StickerId = request.StickerId,
                    FrameColor = request.FrameColor,
                    IncludeTimestamp = request.IncludeTimestamp,
                    LayoutType = request.LayoutType
                };

                // Pass user.Id explicitly as second argument
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
            {
                contentType = "application/octet-stream"; 
            }

            var bytes = System.IO.File.ReadAllBytes(file);
            var fileName = Path.GetFileName(file);
            return File(bytes, contentType, fileName); 
        }
    }
}
