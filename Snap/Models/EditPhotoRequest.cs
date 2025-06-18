namespace Snap.Models
{
    public class EditPhotoRequest
    {        
        public string SessionId { get; set; }
        public string LayoutType { get; set; }
        public int? FilterId { get; set; }
        public int? StickerId { get; set; }
        public bool IncludeTimestamp { get; set; }
        public string FrameColor { get; set; }        
    }
}
