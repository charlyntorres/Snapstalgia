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

namespace Snap.Services
{
    public class FinalImageService : IFinalImageService
    {
        private readonly string TempFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "temp");
        private readonly string FinalFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "final");
        private readonly string StickerFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "stickers");

        public async Task<string> GenerateFinalImageAsync(FinalImageRequest request)
        {
            if (!Directory.Exists(FinalFolder))
                Directory.CreateDirectory(FinalFolder);

            var sessionFolder = System.IO.Path.Combine(TempFolder, request.SessionId);
            if (!Directory.Exists(sessionFolder))
                throw new Exception("Session images folder not found.");

            var imageFiles = Directory.GetFiles(sessionFolder, "*.jpg")
                .OrderBy(f => f)
                .ToList();

            var (rows, cols) = LayoutPresets.GetGrid(request.LayoutType);
            var expectedCount = rows * cols;

            if (imageFiles.Count < expectedCount)
                throw new Exception($"Not enough images for layout. Expected {expectedCount}, found {imageFiles.Count}.");

            // Load images
            var images = imageFiles.Take(expectedCount)
                .Select(f => Image.Load<Rgba32>(f))
                .ToList();

            int imgWidth = images[0].Width;
            int imgHeight = images[0].Height;

            int finalWidth = cols * imgWidth;
            int finalHeight = rows * imgHeight;

            using var finalImage = new Image<Rgba32>(finalWidth, finalHeight);

            // Compose final image grid
            for (int i = 0; i < images.Count; i++)
            {
                int x = (i % cols) * imgWidth;
                int y = (i / cols) * imgHeight;

                images[i].Mutate(ctx => ctx.ApplyFilter(request.FilterId));
                finalImage.Mutate(ctx => ctx.DrawImage(images[i], new Point(x, y), 1f));

                images[i].Dispose(); // Free memory
            }

            // Frame
            if (!string.IsNullOrWhiteSpace(request.FrameColor))
            {
                try
                {
                    var frameColor = Color.ParseHex(request.FrameColor);
                    int thickness = 10;
                    finalImage.Mutate(ctx =>
                    {
                        ctx.Draw(frameColor, thickness, new RectangularPolygon(0, 0, finalWidth, finalHeight));
                    });
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Invalid frame color string: '{request.FrameColor}'.", ex);
                }
            }

            // Sticker
            if (request.StickerId.HasValue)
            {
                string stickerPath = System.IO.Path.Combine(StickerFolder, $"{request.StickerId.Value}.png");
                if (File.Exists(stickerPath))
                {
                    using var stickerImage = Image.Load<Rgba32>(stickerPath);
                    int posX = finalWidth - stickerImage.Width - 10;
                    int posY = finalHeight - stickerImage.Height - 10;

                    finalImage.Mutate(ctx => ctx.DrawImage(stickerImage, new Point(posX, posY), 1f));
                }
            }

            // Timestamp
            if (request.IncludeTimestamp)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var fontPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Assets", "Fonts", "BricolageGrotesque-Regular.ttf");
                var font = TryLoadFont(fontPath, 18);

                var textOptions = new RichTextOptions(font)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Origin = new PointF(finalWidth - 10, finalHeight - 10)
                };

                finalImage.Mutate(ctx => ctx.DrawText(textOptions, timestamp, Color.White));
            }

            var fileName = $"{request.SessionId}_final_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            var filePath = System.IO.Path.Combine(FinalFolder, fileName);
            await finalImage.SaveAsJpegAsync(filePath);

            return $"/images/final/{fileName}";
        }

        private static Font TryLoadFont(string fontPath, float size)
        {
            if (!File.Exists(fontPath))
                throw new FileNotFoundException($"Font file not found at path: {fontPath}");

            var fontCollection = new FontCollection();
            var fontFamily = fontCollection.Add(fontPath);
            return fontFamily.CreateFont(size);
        }
    }
}
