using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Snap.Controllers
{
    //[Authorize]
    public class ProfileController : Controller
    {
        // Main profile page
        public IActionResult ProfilePage()
        {
            return View();
        }

        // Photostrip view page
        public IActionResult PhotostripView()
        {
            return View();
        }
    }
}
