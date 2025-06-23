using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Snap.Controllers
{
    //[Authorize]
    public class ProfileController : Controller
    {
        public IActionResult ProfilePage()
        {
            return View();
        }
    }
}
