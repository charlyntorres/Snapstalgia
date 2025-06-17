using System;
using System.Collections.Generic;
using System.IO;

namespace Snap.Helpers
{
    public static class StickerPresets
    {
        private static readonly Dictionary<int, string> StickerMap = new()
        {
            { 1, "sparkle.png" },
            { 2, "heart.png" },
            { 3, "star.png" }            
        };
        
        public static bool TryGetStickerPath(int? stickerId, out string relativePath)
        {
            relativePath = null;

            if (!stickerId.HasValue || !StickerMap.TryGetValue(stickerId.Value, out var fileName))
                return false;
            
            relativePath = Path.Combine("stickers", fileName);
            return true;
        }
    }
}
