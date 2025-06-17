using Microsoft.AspNetCore.Mvc;

namespace Snap.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult AboutPage()
        {
            return View();
        }
    }
}
