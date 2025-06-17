using Microsoft.AspNetCore.Mvc;

namespace Snap.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult ProfilePage()
        {
            return View();
        }
    }
}
