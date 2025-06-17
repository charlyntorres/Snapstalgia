using Microsoft.AspNetCore.Mvc;

namespace Snap.Controllers
{
    public class PrivacyPolicyController : Controller
    {
        public IActionResult PrivacyPolicyPage()
        {
            return View();
        }
    }
}
