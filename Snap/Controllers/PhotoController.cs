using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Snap.Models;
using Snap.Helpers;

namespace Snap.Controllers
{
    public class PhotoController : Controller
    {
        [HttpPost]
        //[ValidateAntiForgeryToken] // Remove when testing
        public async Task<IActionResult> UploadPhoto([FromBody] PhotoUploadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Base64Image) || string.IsNullOrWhiteSpace(request.SessionId))            
                return BadRequest("Invalid photo upload request.");            

            try
            {
                // Validate layout type
                var dimensions = LayoutPresets.GetDimensions(request.LayoutType);
                var grid = LayoutPresets.GetGrid(request.LayoutType);

                // Override dimensions if provided
                request.Width = dimensions.Width;
                request.Height = dimensions.Height;

                var base64Data = request.Base64Image.Contains(",")
                    ? request.Base64Image.Split(',')[1] 
                    : request.Base64Image;

                var imageBytes = Convert.FromBase64String(base64Data);

                var timestamp = DateTime.UtcNow;
                var fileName = $"{request.SessionId}_{request.Sequence}_{timestamp:yyyyMMdd_HHmmss}.jpg";

                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "temp");
                if (!Directory.Exists(folderPath))                
                    Directory.CreateDirectory(folderPath);      
                
                var filePath = Path.Combine(folderPath, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                var photo = new CapturedPhoto
                {
                    FileName = fileName,
                    CapturedAt = timestamp,
                    SessionId = request.SessionId,
                    Sequence = request.Sequence,
                    LayoutType = request.LayoutType,
                    Width = request.Width,
                    Height = request.Height
                };

                return Ok(photo);
            }
            catch (NotImplementedException)
            {
                return BadRequest($"Unsupported layout type: {request.LayoutType}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Upload error: {ex.Message}.");
            }
        }
    }
}
