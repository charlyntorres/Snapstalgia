using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Snap.Controllers
{
    //[Authorize]
    public class ChooseLayoutController : Controller
    {
        public IActionResult ChooseLayoutPage()
        {
            return View();
        }
    }
}
