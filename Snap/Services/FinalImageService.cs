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

            var imageFiles = Directory.GetFiles(sessionFolder, "*.png")
                .OrderBy(f => f)
                .ToList();

            var (rows, cols) = LayoutPresets.GetGrid(request.LayoutType);
            var expectedCount = rows * cols;

            if (imageFiles.Count < expectedCount)
                throw new Exception($"Not enough images for layout. Expected {expectedCount}, found {imageFiles.Count}.");

            // Filter
            var images = new List<Image<Rgba32>>();
            foreach (var file in imageFiles.Take(expectedCount))
            {
                var image = Image.Load<Rgba32>(file);

                switch (request.FilterId)
                {
                    case 1:
                        ApplyPinterest2Filter(image);  // Retro Pop
                        break;
                    case 2:
                        ApplyPinterest4Filter(image);  // Vintage Film
                        break;
                    case 3:
                        ApplyPinterest5Filter(image);  // Retro Sunset
                        break;
                    case 4:
                        ApplyPinterest8Filter(image);   // Polaroid Hush
                        break;
                    case 5:
                        ApplyPinterest13Filter(image);  // Moody Vinyl
                        break;
                    default:
                        break;
                }

                images.Add(image);
            }

            int photoWidth = images[0].Width;
            int photoHeight = images[0].Height;
            int spacing = 13;
            int topMargin = 15;
            int leftMargin = 12;

            var (finalWidth, finalHeight) = LayoutPresets.GetFinalImageSize(request.LayoutType);

            // Frame color
            var allowedColors = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "BA5E62",   // Muted rose
                "EFA5A6",   // Light pink
                "F9E5DA",   // Soft peach
                "69AFAD",   // Cool mint
                "354E52",   // Dark teal
                "1E1E1E",   // Classic black
                "97C78E",   // Light green
                "CC6B49",   // Burnt orange
                "D2A24C",   // Warm gold
                "ECE6C2",   // Pale beige
                "6F5643",   // Coffee brown
                "C2DFF3"    // Baby blue
            };

            var inputColor = request.FrameColor?.Trim().TrimStart('#').ToUpperInvariant();

            var colorHex = !string.IsNullOrEmpty(inputColor) && allowedColors.Contains(inputColor)
                ? inputColor
                : "BA5E62";

            var frameColor = Color.ParseHex("#" + colorHex);

            Color textColor;
            if (GetLuminance(frameColor) < 0.5)
            {
                textColor = Color.ParseHex("F9E5DA");
            }
            else
            {
                textColor = Color.ParseHex("1E1E1E");
            }

            using var finalImage = new Image<Rgba32>(finalWidth, finalHeight);
            finalImage.Mutate(ctx => ctx.Clear(frameColor));

            // Stickers
            string behindPath = null;
            string frontPath = null;

            // Behind overlay
            if (request.StickerId.HasValue && StickerPresets.TryGetStickerPaths(request.LayoutType, request.StickerId, out behindPath, out frontPath))
            {
                if (!string.IsNullOrWhiteSpace(behindPath))
                {
                    var behindFullPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", behindPath);
                    if (File.Exists(behindFullPath))
                    {
                        using var behindOverlay = Image.Load<Rgba32>(behindFullPath);
                        behindOverlay.Mutate(x => x.Resize(finalWidth, finalHeight));
                        finalImage.Mutate(ctx => ctx.DrawImage(behindOverlay, 1f));
                    }
                }
            }

            // Photo grid
            for (int i = 0; i < images.Count; i++)
            {
                int x = leftMargin;
                int y = topMargin + i * (photoHeight + spacing);

                finalImage.Mutate(ctx => ctx.DrawImage(images[i], new Point(x, y), 1f));
                images[i].Dispose();
            }

            // Front overlay
            if (!string.IsNullOrWhiteSpace(frontPath))
            {
                var frontFullPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", frontPath);
                if (File.Exists(frontFullPath))
                {
                    using var frontOverlay = Image.Load<Rgba32>(frontFullPath);
                    frontOverlay.Mutate(x => x.Resize(finalWidth, finalHeight));
                    finalImage.Mutate(ctx => ctx.DrawImage(frontOverlay, 1f));
                }
            }

            // Brand label
            var brand = "SnapStalgia";
            var brandFontPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Assets", "Fonts", "TR Candice.TTF");
            var brandFontLabel = TryLoadFont(brandFontPath, 16);
            var brandSize = TextMeasurer.MeasureSize(brand, new TextOptions(brandFontLabel));
            var brandX = (finalWidth - brandSize.Width) / 2;
            var brandY = finalHeight - 43;

            finalImage.Mutate(ctx =>
            {
                ctx.DrawText(brand, brandFontLabel, textColor, new PointF(brandX, brandY));
            });           

            // Timestamp
            if (request.IncludeTimestamp)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var timeFontPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Assets", "Fonts", "BricolageGrotesque-Regular.ttf");
                var font = TryLoadFont(timeFontPath, 8);

                var textSize = TextMeasurer.MeasureSize(timestamp, new TextOptions(font));
                var timestampX = (finalWidth - textSize.Width) / 2;
                var timestampY = finalHeight - 23;

                finalImage.Mutate(ctx =>
                {
                    ctx.DrawText(timestamp, font, textColor, new PointF(timestampX, timestampY));
                });
            }

            // Save
            var fileName = $"{request.SessionId}_final_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var filePath = System.IO.Path.Combine(FinalFolder, fileName);
            await finalImage.SaveAsPngAsync(filePath);

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

        // FILTER PINTEREST 2
        private void ApplyPinterest2Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Contrast(1.3f)
                   .Saturate(1.5f)
                   .Brightness(1.1f)
                   .Hue(15);

                var overlayColor = Color.FromRgba(255, 204, 153, 50);
                ctx.Fill(overlayColor);
            });

            AddGrainNoise(image, intensity: 0.03f);
        }

        // FILTER PINTEREST 4
        private void ApplyPinterest4Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Sepia()
                   .Saturate(0.85f)
                   .Contrast(0.9f)
                   .Brightness(1.05f)
                   .Hue(15)
                   .Vignette(Color.FromRgba(0, 0, 0, 100));
            });

            AddGrainNoise(image, intensity: 0.015f);
        }

        // FILTER PINTEREST 5
        private static void ApplyPinterest5Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Hue(25)
                   .Saturate(1.8f)
                   .Contrast(1.2f)
                   .Brightness(1.05f);

                var gradient = new LinearGradientBrush(
                    new PointF(0, 0),
                    new PointF(0, image.Height),
                    GradientRepetitionMode.None,
                    new ColorStop(0f, Color.FromRgba(255, 183, 76, 70)),
                    new ColorStop(1f, Color.FromRgba(255, 94, 151, 70))  
                );

                ctx.Fill(gradient);
            });
        }

        // FILTER PINTEREST 8
        private void ApplyPinterest8Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Brightness(0.9f)              
                   .Contrast(0.9f)                
                   .Saturate(1.24f)               
                   .Hue(-10f);                    

                var greenShadow = Color.FromRgba(180, 255, 200, 25);
                ctx.Fill(greenShadow);

                ctx.Vignette(Color.FromRgba(0, 0, 0, 80));
            });

            AddGrainNoise(image, 0.03f);
        }

        // FILTER PINTEREST 13
        private void ApplyPinterest13Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Saturate(0.7f)
                   .Contrast(1.5f)
                   .Brightness(0.95f)
                   .Vignette(Color.FromRgba(0, 0, 0, 100));
            });

            AddGrainNoise(image, 0.02f);
        }

        // Grain Helper 1
        private void AddGrainNoise(Image<Rgba32> image, float intensity = 0.05f)
        {
            var random = new Random();
            int width = image.Width;
            int height = image.Height;

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < width; x++)
                    {
                        var pixel = row[x];

                        float noise = (float)(random.NextDouble() * 2 - 1) * intensity;

                        pixel.R = ClampByte(pixel.R + (int)(noise * 255));
                        pixel.G = ClampByte(pixel.G + (int)(noise * 255));
                        pixel.B = ClampByte(pixel.B + (int)(noise * 255));

                        row[x] = pixel;
                    }
                }
            });
        }

        // Grain Helper 2
        private byte ClampByte(int value)
        {
            return (byte)Math.Clamp(value, 0, 255);
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