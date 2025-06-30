using Snap.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Drawing;
using Snap.Helpers;
using SixLabors.ImageSharp.Processing.Processors.Filters;
using SixLabors.ImageSharp.Processing.Processors;
using Snap.Helpers;
using Snap.Areas.Identity.Data.Data;

namespace Snap.Services
{
    public class FinalImageService : IFinalImageService
    {
        private readonly string TempFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "temp");
        private readonly string FinalFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "final");
        private readonly ApplicationDbContext _context;

        public FinalImageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateFinalImageAsync(FinalImageRequest request, string userId)
        {
            if (!Directory.Exists(FinalFolder))
                Directory.CreateDirectory(FinalFolder);

            var sessionFolder = System.IO.Path.Combine(TempFolder, request.SessionId);
            if (!Directory.Exists(sessionFolder))
                throw new Exception("Session images folder not found.");

            var imageFiles = Directory.GetFiles(sessionFolder, "*.png").OrderBy(f => f).ToList();

            var (rows, cols) = LayoutPresets.GetGrid(request.LayoutType);
            var expectedCount = rows * cols;

            if (imageFiles.Count < expectedCount)
                throw new Exception($"Not enough images for layout. Expected {expectedCount}, found {imageFiles.Count}.");

            var images = imageFiles.Take(expectedCount).Select(path =>
            {
                var image = Image.Load<Rgba32>(path);
                image.ApplyFilter(request.FilterId ?? 0);
                return image;
            }).ToList();

            int photoWidth = images[0].Width;
            int photoHeight = images[0].Height;
            int spacing = 13;
            int topMargin = 15;
            int leftMargin = 12;

            var (finalWidth, finalHeight) = LayoutPresets.GetFinalImageSize(request.LayoutType);

            // Frame color
            var allowedColors = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "BA5E62", "EFA5A6", "F9E5DA", "69AFAD", "354E52", "1E1E1E", "97C78E", "CC6B49", "D2A24C", "ECE6C2", "6F5643", "C2DFF3"
            };

            var inputColor = request.FrameColor?.Trim().TrimStart('#').ToUpperInvariant();
            var colorHex = !string.IsNullOrEmpty(inputColor) && allowedColors.Contains(inputColor)
                ? inputColor
                : "BA5E62";

            var frameColor = Color.ParseHex("#" + colorHex);
            var textColor = GetLuminance(frameColor) < 0.5 ? Color.ParseHex("F9E5DA") : Color.ParseHex("354E52");

            using var finalImage = new Image<Rgba32>(finalWidth, finalHeight);
            finalImage.Mutate(ctx => ctx.Clear(frameColor));

            string behindPath = null;
            string frontPath = null;

            // Behind Sticker
            if (request.StickerId.HasValue && StickerPresets.TryGetStickerPaths(request.LayoutType, request.StickerId, out behindPath, out frontPath))
            {
                if (!string.IsNullOrWhiteSpace(behindPath))
                {
                    var behindFullPath = System.IO.Path.Combine("wwwroot", behindPath);
                    if (File.Exists(behindFullPath))
                    {
                        using var behindOverlay = Image.Load<Rgba32>(behindFullPath);
                        behindOverlay.Mutate(x => x.Resize(finalWidth, finalHeight));
                        finalImage.Mutate(ctx => ctx.DrawImage(behindOverlay, 1f));
                    }
                }
            }

            // Paste photos
            for (int i = 0; i < images.Count; i++)
            {
                int x = leftMargin;
                int y = topMargin + i * (photoHeight + spacing);
                finalImage.Mutate(ctx => ctx.DrawImage(images[i], new Point(x, y), 1f));
                images[i].Dispose();
            }

            // Front Sticker
            if (!string.IsNullOrWhiteSpace(frontPath))
            {
                var frontFullPath = System.IO.Path.Combine("wwwroot", frontPath);
                if (File.Exists(frontFullPath))
                {
                    using var frontOverlay = Image.Load<Rgba32>(frontFullPath);
                    frontOverlay.Mutate(x => x.Resize(finalWidth, finalHeight));
                    finalImage.Mutate(ctx => ctx.DrawImage(frontOverlay, 1f));
                }
            }

            // Brand Text
            var brandFont = TryLoadFont(System.IO.Path.Combine("wwwroot", "Assets", "Fonts", "TR Candice.TTF"), 16);
            var brandText = "SnapStalgia";
            var brandSize = TextMeasurer.MeasureSize(brandText, new TextOptions(brandFont));
            var brandX = (finalWidth - brandSize.Width) / 2;
            var brandY = finalHeight - 43;

            finalImage.Mutate(ctx => ctx.DrawText(brandText, brandFont, textColor, new PointF(brandX, brandY)));

            // Timestamp
            if (request.IncludeTimestamp)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var timeFont = TryLoadFont(System.IO.Path.Combine("wwwroot", "Assets", "Fonts", "BricolageGrotesque-Regular.ttf"), 8);
                var size = TextMeasurer.MeasureSize(timestamp, new TextOptions(timeFont));
                var x = (finalWidth - size.Width) / 2;
                var y = finalHeight - 23;

                finalImage.Mutate(ctx => ctx.DrawText(timestamp, timeFont, textColor, new PointF(x, y)));
            }

            var timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var fileName = $"Photostrip_{timeStamp}.png";
            var finalPath = System.IO.Path.Combine(FinalFolder, fileName);
            await finalImage.SaveAsPngAsync(finalPath);

            // Save record to DB
            var photo = new Photo
            {
                UserId = userId,
                FilePath = "/images/final/" + fileName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();
            Console.WriteLine("Saving photo for userId: " + userId);
            Console.WriteLine("Final image path: " + "/images/final/" + fileName);


            return photo.FilePath;
        }

        private static Font TryLoadFont(string fontPath, float size)
        {
            if (!File.Exists(fontPath))
                throw new FileNotFoundException($"Font file not found at path: {fontPath}");

            var fontCollection = new FontCollection();
            var fontFamily = fontCollection.Add(fontPath);
            return fontFamily.CreateFont(size);
        }

        // Text Color Helper
        private static double GetLuminance(Color color)
        {
            var rgba = color.ToPixel<Rgba32>();
            return 0.2126 * rgba.R / 255
                + 0.7152 * rgba.G / 255
                + 0.0722 * rgba.B / 255;
        }
    }
}
