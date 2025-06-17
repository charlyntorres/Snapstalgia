using Microsoft.AspNetCore.Mvc;

namespace Snap.Controllers
{
    public class ChooseLayoutController : Controller
    {
        public IActionResult ChooseLayoutPage()
        {
            return View();
        }
    }
}
