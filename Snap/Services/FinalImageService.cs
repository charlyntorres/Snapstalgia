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
using System;

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

            var images = new List<Image<Rgba32>>();
            foreach (var file in imageFiles.Take(expectedCount))
            {
                var image = Image.Load<Rgba32>(file);

                switch (request.FilterId)
                {
                    case 1:
                        ApplyPinterest1Filter(image);   // 90s vintage cam
                        break;
                    case 2:
                        ApplyPinterest2Filter(image);  // RetroPop
                        break;
                    case 3:
                        ApplyPinterest3Filter(image);   // Japancore
                        break;
                    case 4:
                        ApplyPinterest4Filter(image);  // VintageFilm
                        break;
                    case 5:
                        ApplyPinterest5Filter(image);  // RetroSunset
                        break;
                    case 6:
                        ApplyPinterest6Filter(image);    // VSCO dispo c1
                        break;
                    case 7:
                        ApplyPinterest7Filter(image);   // Retro 14+7.0
                        break;
                    case 8:
                        ApplyPinterest8Filter(image);   // G3 portraits
                        break;
                    case 9:
                        ApplyPinterest9Filter(image);   //M4 mood
                        break;                                       
                    case 10:
                        ApplyPinterest10Filter(image);  // BubblegumPop
                        break;
                    case 11:
                        ApplyPinterest11Filter(image); // 90sVHS
                        break;
                    case 12:
                        ApplyPinterest12Filter(image);  // InstantFilm
                        break;
                    case 13:
                        ApplyPinterest13Filter(image);  // GrungeFade
                        break;
                    case 14:
                        ApplyPinterest14Filter(image);  // DustyRose
                        break;
                    case 15:
                        ApplyPinterest15Filter(image);  // MidnightMood
                        break;
                    case 16:
                        ApplyPinterest16Filter(image);  // HoneyDrip
                        break;
                    case 17:
                        ApplyPinterest17Filter(image);  // FilmNoir
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

            var frameColor = !string.IsNullOrWhiteSpace(request.FrameColor)
                ? Color.ParseHex(request.FrameColor)
                : Color.White;

            using var finalImage = new Image<Rgba32>(finalWidth, finalHeight);
            finalImage.Mutate(ctx => ctx.Clear(frameColor));

            // Photo grid
            for (int i = 0; i < images.Count; i++)
            {
                int x = leftMargin;
                int y = topMargin + i * (photoHeight + spacing);               

                finalImage.Mutate(ctx => ctx.DrawImage(images[i], new Point(x, y), 1f));
                images[i].Dispose();
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

                var textSize = TextMeasurer.MeasureSize(timestamp, new TextOptions(font));
                var timestampX = (finalWidth - textSize.Width) / 2;
                var timestampY = finalHeight - 28;

                finalImage.Mutate(ctx =>
                {
                    ctx.DrawText(
                        timestamp,
                        font,
                        Color.Black,
                        new PointF(timestampX, timestampY));
                });
            }

            // Save
            var fileName = $"{request.SessionId}_final_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            var filePath = System.IO.Path.Combine(FinalFolder, fileName);
            await finalImage.SaveAsJpegAsync(filePath);

            Console.WriteLine($"Final image size: {finalWidth}x{finalHeight}px");

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

        // GRAIN REMOVE IF PANGET
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

        // PART OF GRAIN, REMOVE IF PANGET
        private byte ClampByte(int value)
        {
            return (byte)Math.Clamp(value, 0, 255);
        }

        // FITLER PINTEREST 1
        private void ApplyPinterest1Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Brightness(1.15f)
                   .Contrast(0.8f)
                   .Saturate(1.5f)
                   .GaussianSharpen(1.5f)
                   .Vignette(Color.FromRgba(0, 0, 0, 150));
            });

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

        // FILTER PINTEREST 3
        private void ApplyPinterest3Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Brightness(1.22f)            
                   .Contrast(0.72f)              
                   .Saturate(1.09f)              
                   .GaussianSharpen(1.1f);     
            });

            AddGrainNoise(image, 0.012f);
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

        // FILTER PINTEREST 6
        private void ApplyPinterest6Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Brightness(1.05f)              
                   .Contrast(1.25f)               
                   .Saturate(1.15f)                
                   .Hue(4f)                        
                   .GaussianSharpen(1.0f);         

                var greenishOverlay = Color.FromRgba(160, 255, 200, 20); 
                ctx.Fill(greenishOverlay);
            });

            var fadeOverlay = Color.FromRgba(255, 255, 255, 20);
            image.Mutate(ctx => ctx.Fill(fadeOverlay));

            image.Mutate(ctx => ctx.Vignette(Color.FromRgba(0, 0, 0, 90)));

            AddGrainNoise(image, 0.01f);
        }

        // FILTER PINTEREST 7
        private static void ApplyPinterest7Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Brightness(1.05f)             
                   .Contrast(0.85f)              
                   .Saturate(1.1f)               
                   .Hue(10f)                      
                   .GaussianSharpen(0.8f);        

                var overlay = Color.FromRgba(255, 240, 180, 30);
                ctx.Fill(overlay);
            });

            image.Mutate(ctx => ctx.Fill(Color.FromRgba(255, 255, 255, 35))); 
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

        // FILTER PINTEREST 9
        private void ApplyPinterest9Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Brightness(0.95f)             
                   .Contrast(1.1f)                
                   .Saturate(1.0f)                
                   .Hue(-3f);                     
                
                var brownTone = Color.FromRgba(180, 140, 100, 35);
                ctx.Fill(brownTone);
                
                ctx.Lightness(1.1f);
            });

            AddGrainNoise(image, 0.02f);
        }

        // FILTER PINTEREST 10
        private void ApplyPinterest10Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Hue(320)
                   .Saturate(1.8f)
                   .Brightness(1.2f)
                   .Contrast(0.9f);

                var overlayColor = Color.FromRgba(255, 182, 193, 50); 
                ctx.Fill(overlayColor);
            });

            AddGrainNoise(image);
        }

        // FILTER PINTEREST 11
        private void ApplyPinterest11Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Hue(285) 
                   .Brightness(1.1f)
                   .Contrast(1.3f)
                   .Saturate(1.2f);
            });

            AddGrainNoise(image, 0.025f);
        }

        // FILTER PINTEREST 12
        private void ApplyPinterest12Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Brightness(1.1f)
                   .Contrast(1.0f)
                   .Saturate(0.9f)
                   .Hue(35); 
            });

            AddGrainNoise(image, 0.01f);
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

        // FILTER PINTEREST 14
        private void ApplyPinterest14Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Hue(330)
                   .Saturate(0.75f)
                   .Brightness(1.0f)
                   .Contrast(0.9f);
            });

            AddGrainNoise(image, 0.012f);
        }

        // FILTER PINTEREST 15
        private static void ApplyPinterest15Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Hue(220)              
                   .Contrast(1.3f)
                   .Brightness(0.95f)
                   .Saturate(0.9f);

                ctx.Vignette(Color.FromRgba(0, 0, 40, 100));
            });            
        }

        // FILTER PINTEREST 16
        private static void ApplyPinterest16Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Hue(40)
                   .Saturate(1.2f)
                   .Brightness(1.1f)
                   .Contrast(1.1f);
            });

            var overlay = Color.FromRgba(255, 223, 140, 50); 
            image.Mutate(ctx => ctx.Fill(overlay));
        }

        // FILTER PINTEREST 17
        private static void ApplyPinterest17Filter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Grayscale()
                   .Brightness(0.9f)
                   .Contrast(1.5f);
                
                ctx.Vignette(Color.FromRgba(0, 0, 0, 120)); 
            });           
        }

    }
}