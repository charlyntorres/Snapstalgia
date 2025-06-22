using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Azure.Core;

namespace Snap.Helpers
{
    public static class FilterPresets
    {
        public static void ApplyFilter(this Image<Rgba32> image, int filterId)
        {
            switch (filterId)
            {
                case 1:
                    ApplyRetroPopFilter(image);
                    break;
                case 2:
                    ApplyVintageFilmFilter(image);
                    break;
                case 3:
                    ApplyRetroSunsetFilter(image);
                    break;
                case 4:
                    ApplyPolaroidHushFilter(image);
                    break;
                case 5:
                    ApplyMoodyVinylFilter(image);
                    break;
                default:
                    break;
            }
        }

        // ApplyRetroPopFilter
        private static void ApplyRetroPopFilter(Image<Rgba32> image)
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

        // ApplyVintageFilmFilter
        private static void ApplyVintageFilmFilter(Image<Rgba32> image)
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

        // ApplyRetroSunsetFilter
        private static void ApplyRetroSunsetFilter(Image<Rgba32> image)
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

        // ApplyPolaroidHushFilter
        private static void ApplyPolaroidHushFilter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Brightness(1.0f)
                   .Contrast(0.9f)
                   .Saturate(1.24f)
                   .Hue(-10f);

                var greenShadow = Color.FromRgba(180, 255, 200, 25);
                ctx.Fill(greenShadow);

                ctx.Vignette(Color.FromRgba(0, 0, 0, 80));
            });

            AddGrainNoise(image, 0.03f);
        }

        // ApplyMoodyVinylFilter
        private static void ApplyMoodyVinylFilter(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Saturate(0.7f)
                   .Contrast(1.5f)
                   .Brightness(0.95f)
                   .Vignette(Color.FromRgba(0, 0, 0, 90));
            });

            AddGrainNoise(image, 0.02f);
        }

        // Grain Helper 1
        private static void AddGrainNoise(Image<Rgba32> image, float intensity = 0.05f)
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
        private static byte ClampByte(int value)
        {
            return (byte)Math.Clamp(value, 0, 255);
        }
    }
}