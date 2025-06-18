using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Snap.Models;
using Snap.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.Fonts;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;

namespace Snap.Controllers
{
    public class FinalImageController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> FinalizeImage([FromBody] FinalizeRequest request)
        {
            var tempFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "temp");
            var finalFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "final");

            if (!Directory.Exists(finalFolder))
                Directory.CreateDirectory(finalFolder);

            var originalPath = System.IO.Path.Combine(tempFolder, request.FileName);
            if (!System.IO.File.Exists(originalPath))
                return NotFound("Original photo not found.");

            var finalFileName = "final_" + request.FileName;
            var finalPath = System.IO.Path.Combine(finalFolder, finalFileName);

            using var image = await Image.LoadAsync(originalPath);

            // Apply filter            
            image.Mutate(x => x.ApplyFilter(request.FilterId));

            // Add sticker overlay
            if (StickerPresets.TryGetStickerPath(request.StickerId, out var stickerRelativePath))
            {
                var stickerFullPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", stickerRelativePath);
                if (System.IO.File.Exists(stickerFullPath))
                {
                    using var sticker = await Image.LoadAsync(stickerFullPath);
                    sticker.Mutate(x => x.Resize(image.Width / 4, 0));
                    image.Mutate(x => x.DrawImage(sticker, new Point(image.Width - sticker.Width - 10, 10), 1f));
                }
            }


            // Apply frame color
            if (!string.IsNullOrWhiteSpace(request.FrameColor))
            {
                var borderColor = Color.Parse(request.FrameColor);
                int borderThickness = 20;

                image.Mutate(ctx =>
                {
                    // Top
                    ctx.Fill(borderColor, new RectangleF(0, 0, image.Width, borderThickness));
                    // Bottom
                    ctx.Fill(borderColor, new RectangleF(0, image.Height - borderThickness, image.Width, borderThickness));
                    // Left
                    ctx.Fill(borderColor, new RectangleF(0, 0, borderThickness, image.Height));
                    // Right
                    ctx.Fill(borderColor, new RectangleF(image.Width - borderThickness, 0, borderThickness, image.Height));
                });
            }


            // Add timestamp
            if (request.IncludeTimestamp)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                var font = SystemFonts.CreateFont("Arial", 24);
                image.Mutate(x => x.DrawText(timestamp, font, Color.White, new PointF(10, image.Height - 40)));
            }

            await image.SaveAsync(finalPath);

            return Ok(new { FinalFileName = finalFileName });
        }
    }
}
