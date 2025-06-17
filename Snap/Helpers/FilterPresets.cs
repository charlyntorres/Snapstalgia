using SixLabors.ImageSharp.Processing;

namespace Snap.Helpers
{
    public static class FilterPresets
    {
        public static IImageProcessingContext ApplyFilter(this IImageProcessingContext context, int? filterId)
        {
            if (!filterId.HasValue)          
                return context;

            return filterId.Value switch
            {
                1 => context.Grayscale(),
                2 => context.Sepia(),
                3 => context.Brightness(1.25f),
                _ => context,
            };
        }
    }
}