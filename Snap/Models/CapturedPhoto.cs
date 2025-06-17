using System;

namespace Snap.Models  // Replace with your actual namespace
{
    public class CapturedPhoto
    {
        public string FileName { get; set; }        // The name of the captured photo file
        public DateTime CapturedAt { get; set; }    // Timestamp when the photo was captured
        public string SessionId { get; set; }       // To group all photos from the same layout session
        public int Sequence { get; set; }           // Sequence number for the photo in the session
        public string LayoutType { get; set; }      // (Optional now, but useful for organizing layout logic)
        public int Width { get; set; }              // Dimensions of the captured photo
        public int Height { get; set; }             // Dimensions of the captured photo
    }
}
