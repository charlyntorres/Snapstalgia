using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Snap.ViewModels;
using Snap.Areas.Identity.Data.Data;

namespace Snap.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Profile page
        public async Task<IActionResult> ProfilePage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var photos = await _context.Photos
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var viewModel = new ProfileViewModel
            {
                Username = user.UserName,
                Email = user.Email,
                PhotoStrips = photos
            };

            return View(viewModel);
        }

        // View full photostrip
        public async Task<IActionResult> PhotostripView(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var photo = await _context.Photos
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);

            if (photo == null) return NotFound();

            return View(photo);
        }

        // Delete photostrip
        [HttpPost]
        public async Task<IActionResult> DeletePhotostrip(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var photo = await _context.Photos
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);

            if (photo == null) return NotFound();
            
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", photo.FilePath.TrimStart('~', '/'));
            if (System.IO.File.Exists(filePath))            
                System.IO.File.Delete(filePath);            

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            return RedirectToAction("ProfilePage");
        }
    }
}
