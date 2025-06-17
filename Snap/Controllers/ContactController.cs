using Microsoft.AspNetCore.Mvc;

namespace Snap.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult ContactPage()
        {
            return View();
        }
    }
}
