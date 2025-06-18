namespace Snap.Models
{
    public class EditPhotoRequest
    {
        public string EditedBase64Image { get; set; }
        public string SessionId { get; set; }
        public int Sequence {  get; set; }
        public string FilterApplied { get; set; }
    }
}
