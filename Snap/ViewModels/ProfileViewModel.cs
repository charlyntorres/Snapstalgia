using Snap.Models;

namespace Snap.ViewModels
{
    public class ProfileViewModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public List<PhotoStripViewModel> PhotoStrips { get; set; }
    }
}
