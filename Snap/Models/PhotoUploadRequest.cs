using System;

namespace Snap.Models // Replace with your actual project namespace
{
    public class PhotoUploadRequest
    {
        public string Base64Image { get; set; }   // The base64-encoded image string from frontend
        public string SessionId { get; set; }     // Unique ID for the photo session
        public int Sequence { get; set; }         // Sequence number for the photo in the session
        public string LayoutType { get; set; }    // Type of layout (e.g., "2x2", "3x3")
    }
}
