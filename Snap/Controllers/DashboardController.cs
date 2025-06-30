using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Snap.Models;
using System.Threading.Tasks;
using System.Linq;
using Snap.Areas.Identity.Data.Data;

namespace Snap.Controllers
{
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DashboardController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var photos = await _context.Photos
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(photos);
        }

        public async Task<IActionResult> Download(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);

            if (photo == null || !System.IO.File.Exists(Path.Combine("wwwroot", photo.FilePath)))
            {
                return NotFound();
            }

            var filePath = Path.Combine("wwwroot", photo.FilePath);
            var contentType = "application/octet-stream";
            var fileName = Path.GetFileName(photo.FilePath);

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, contentType, fileName);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);

            if (photo == null)
                return NotFound();

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", photo.FilePath);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
