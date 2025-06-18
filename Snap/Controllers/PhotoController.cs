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
        public async Task<IActionResult> UploadPhoto([FromBody] PhotoUploadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Base64Image) || string.IsNullOrWhiteSpace(request.SessionId))            
                return BadRequest("Invalid photo upload request.");            

            try
            {
                // Validate layout type
                var (Width, Height) = LayoutPresets.GetDimensions(request.LayoutType);
                var (Rows, Cols) = LayoutPresets.GetGrid(request.LayoutType);

                // Override dimensions if provided
                request.Width = Width;
                request.Height = Height;

                var base64Data = request.Base64Image.Contains(',')
                    ? request.Base64Image.Split(',')[1] 
                    : request.Base64Image;

                var imageBytes = Convert.FromBase64String(base64Data);

                var timestamp = DateTime.Now;
                
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

        [HttpPost]
        public async Task<IActionResult> ApplyEdits([FromForm] EditPhotoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EditedBase64Image))
                return BadRequest("No edited image provided.");

            try
            {
                var base64Data = request.EditedBase64Image.Contains(",")
                    ? request.EditedBase64Image.Split(',')[1]
                    : request.EditedBase64Image;

                var imageBytes = Convert.FromBase64String(base64Data);

                var fileName = $"{request.SessionId}_{request.Sequence}_final_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "final");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                return Ok(new { message = "Edited photo saved successfully", fileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving edited photo: {ex.Message}");
            }
        }
    }
}
