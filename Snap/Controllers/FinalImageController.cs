using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Snap.Helpers;
using Snap.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Snap.Areas.Identity.Data.Data;
using Snap.Services;

namespace Snap.Controllers
{
    //[Authorize]
    public class FinalImageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IFinalImageService _finalImageService;

        public FinalImageController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IFinalImageService finalImageService)
        {
            _userManager = userManager;
            _context = context;
            _finalImageService = finalImageService;
        }
        [HttpPost]
        public async Task<IActionResult> FinalizeImage([FromBody] FinalizeRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            try
            {
                // Map FinalizeRequest to FinalImageRequest
                var finalRequest = new FinalImageRequest
                {
                    SessionId = System.IO.Path.GetFileNameWithoutExtension(request.FileName), // derive sessionId from FileName
                    FilterId = request.FilterId,
                    StickerId = request.StickerId,
                    FrameColor = request.FrameColor,
                    IncludeTimestamp = request.IncludeTimestamp,
                    LayoutType = request.LayoutType
                };

                var filePath = await _finalImageService.GenerateFinalImageAsync(finalRequest, user.Id);

                return Ok(new { FilePath = filePath });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> FinalizeImage([FromBody] FinalizeRequest request)
        //{
        //    var tempFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "temp");
        //    var finalFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "final");

        //    if (!Directory.Exists(finalFolder))
        //        Directory.CreateDirectory(finalFolder);

        //    var originalPath = System.IO.Path.Combine(tempFolder, request.FileName);
        //    if (!System.IO.File.Exists(originalPath))
        //        return NotFound("Original photo not found.");

        //    var fileBaseName = System.IO.Path.GetFileNameWithoutExtension(request.FileName);
        //    var finalFileName = $"{fileBaseName}_final_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        //    var finalPath = System.IO.Path.Combine(finalFolder, finalFileName);

        //    using var image = await Image.LoadAsync<Rgba32>(originalPath);

        //    // Apply filter                        
        //    image.ApplyFilter((int)request.FilterId);

        //    // Add sticker overlays
        //    if (StickerPresets.TryGetStickerPaths(request.LayoutType, request.StickerId, out var behindRelativePath, out var frontRelativePath))
        //    {
        //        // Behind overlay
        //        if (!string.IsNullOrWhiteSpace(behindRelativePath))
        //        {
        //            var behindFullPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", behindRelativePath);
        //            if (System.IO.File.Exists(behindFullPath))
        //            {
        //                using var behindOverlay = await Image.LoadAsync(behindFullPath);
        //                behindOverlay.Mutate(x => x.Resize(image.Width, image.Height)); // match dimensions if needed
        //                image.Mutate(x => x.DrawImage(behindOverlay, 1f));
        //            }
        //        }

        //        // Front overlay
        //        if (!string.IsNullOrWhiteSpace(frontRelativePath))
        //        {
        //            var frontFullPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", frontRelativePath);
        //            if (System.IO.File.Exists(frontFullPath))
        //            {
        //                using var frontOverlay = await Image.LoadAsync(frontFullPath);
        //                frontOverlay.Mutate(x => x.Resize(image.Width, image.Height)); // match dimensions if needed
        //                image.Mutate(x => x.DrawImage(frontOverlay, 1f));
        //            }
        //        }
        //    }

        //    // Apply frame color
        //    if (!string.IsNullOrWhiteSpace(request.FrameColor))
        //    {
        //        var borderColor = Color.Parse(request.FrameColor);
        //        int borderThickness = 20;

        //        image.Mutate(ctx =>
        //        {
        //            ctx.Fill(borderColor, new RectangleF(0, 0, image.Width, borderThickness));
        //            ctx.Fill(borderColor, new RectangleF(0, image.Height - borderThickness, image.Width, borderThickness));
        //            ctx.Fill(borderColor, new RectangleF(0, 0, borderThickness, image.Height));
        //            ctx.Fill(borderColor, new RectangleF(image.Width - borderThickness, 0, borderThickness, image.Height));
        //        });
        //    }

        //    // Add timestamp
        //    if (request.IncludeTimestamp)
        //    {
        //        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        //        var font = SystemFonts.CreateFont("Arial", 24);
        //        image.Mutate(x => x.DrawText(timestamp, font, Color.White, new PointF(10, image.Height - 40)));
        //    }

        //    await image.SaveAsPngAsync(finalPath);

        //    // Save photo to db
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user != null)
        //    {
        //        var relativePath = System.IO.Path.Combine("images", "final", finalFileName).Replace("\\", "/");

        //        var photo = new Photo
        //        {
        //            UserId = user.Id,
        //            FilePath = relativePath,
        //            CreatedAt = DateTime.UtcNow
        //        };

        //        _context.Photos.Add(photo);
        //        await _context.SaveChangesAsync();
        //    }

        //    return Ok(new { FinalFileName = finalFileName });
        //}
    }
}
