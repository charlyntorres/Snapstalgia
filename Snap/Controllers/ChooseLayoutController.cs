using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Snap.Controllers
{
    [Authorize]
    public class ChooseLayoutController : Controller
    {
        // Main layout choosing page
        public IActionResult ChooseLayoutPage()
        {
            return View(); // Views/ChooseLayout/ChooseLayoutPage.cshtml
        }

        // 4-strip version (default)
        public IActionResult TakeAPhotoFour()
        {
            return View("~/Views/ChooseLayout/TakeAPhotoFour.cshtml");
        }

        // 3-strip version
        public IActionResult TakeAPhotoThree()
        {
            return View("~/Views/ChooseLayout/TakeAPhotoThree.cshtml");
        }

        // 2-strip version
        public IActionResult TakeAPhotoTwo()
        {
            return View("~/Views/ChooseLayout/TakeAPhotoTwo.cshtml");
        }

        // Customize photo 2-, 3-, and 4-strips layouts
        public IActionResult Customize1x2photo()
        {
            return View();
        }

        public IActionResult Customize1x3photo()
        {
            return View();
        }

        public IActionResult Customize1x4photo()
        {
            return View();
        }
    }
}
