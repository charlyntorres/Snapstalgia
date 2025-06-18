using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Snap.Models;
using Snap.Services;
using Snap.Helpers;

namespace Snap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoController : ControllerBase
    {
        private readonly IFinalImageService _finalImageService;

        public PhotoController(IFinalImageService finalImageService)
        {
            _finalImageService = finalImageService;
        }

        // POST api/photo/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto([FromForm] PhotoUploadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SessionId) || request.File == null)
                return BadRequest("SessionId and photo file are required.");

            try
            {
                // Validate layout type and get expected grid dimensions
                var (expectedRows, expectedCols) = LayoutPresets.GetGrid(request.LayoutType);

                // Build temp folder path for session
                var tempSessionFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "temp", request.SessionId);
                if (!Directory.Exists(tempSessionFolder))
                    Directory.CreateDirectory(tempSessionFolder);

                // Save uploaded file with sequence number to keep order
                var timestamp = DateTime.Now;
                var fileName = $"{request.SessionId}_{request.Sequence}_{timestamp:yyyyMMdd_HHmmss}.jpg";
                var filePath = Path.Combine(tempSessionFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                var photo = new CapturedPhoto
                {
                    FileName = fileName,
                    CapturedAt = timestamp,
                    SessionId = request.SessionId,
                    Sequence = request.Sequence,
                    LayoutType = request.LayoutType
                };

                return Ok(photo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Upload error: {ex.Message}");
            }
        }

        // POST api/photo/compile
        [HttpPost("compile")]
        public async Task<IActionResult> CompileAndEdit([FromForm] EditPhotoRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.SessionId) || string.IsNullOrWhiteSpace(request.LayoutType))
                    return BadRequest("SessionId and LayoutType are required.");

                // Validate expected number of photos based on layout
                var (rows, cols) = LayoutPresets.GetGrid(request.LayoutType);
                int expectedCount = rows * cols;

                // Check photos exist in temp folder
                var tempSessionFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "temp", request.SessionId);
                if (!Directory.Exists(tempSessionFolder))
                    return BadRequest("No photos uploaded for this session.");

                var images = Directory.GetFiles(tempSessionFolder, "*.jpg");
                if (images.Length < expectedCount)
                    return BadRequest($"Expected {expectedCount} photos but found {images.Length}. Please upload all photos before compiling.");

                // Build FinalImageRequest from EditPhotoRequest
                var finalRequest = new FinalImageRequest
                {
                    SessionId = request.SessionId,
                    FilterId = request.FilterId,
                    StickerId = request.StickerId,
                    FrameColor = request.FrameColor,
                    IncludeTimestamp = request.IncludeTimestamp
                };

                var imagePath = await _finalImageService.GenerateFinalImageAsync(finalRequest);

                return Ok(new { message = "Final image generated successfully.", imagePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error compiling and editing photos: {ex.Message}");
            }
        }
    }
}
