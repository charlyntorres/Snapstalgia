using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Snap.Areas.Identity.Data;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Snap.Areas.Identity.Pages.Account
{
    [Authorize]
    public class UploadPhotoModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UploadPhotoModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public IFormFile ProfileImage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                var user = await _userManager.GetUserAsync(User);
                var uploads = Path.Combine("wwwroot", "images", "profiles");
                Directory.CreateDirectory(uploads);

                var filePath = Path.Combine(uploads, user.Id + Path.GetExtension(ProfileImage.FileName));
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }
            }

            return RedirectToAction("ProfilePage", "Profile");
        }
    }
}
