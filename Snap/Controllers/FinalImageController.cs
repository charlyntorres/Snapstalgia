using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Snap.Models;
using Microsoft.AspNetCore.Identity;
using Snap.Areas.Identity.Data.Data;
using Snap.Services;

namespace Snap.Controllers
{
    [Authorize]
    public class FinalImageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IFinalImageService _finalImageService;

        public FinalImageController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IFinalImageService finalImageService)
        {
            _userManager = userManager;
            _context = context;
            _finalImageService = finalImageService;
        }

        [HttpPost]
        public async Task<IActionResult> FinalizeImage([FromBody] FinalizeRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            try
            {
                var finalRequest = new FinalImageRequest
                {
                    SessionId = System.IO.Path.GetFileNameWithoutExtension(request.FileName), // derive sessionId from FileName
                    FilterId = request.FilterId,
                    StickerId = request.StickerId,
                    FrameColor = request.FrameColor,
                    IncludeTimestamp = request.IncludeTimestamp,
                    LayoutType = request.LayoutType
                };

                var filePath = await _finalImageService.GenerateFinalImageAsync(finalRequest, user.Id);

                return Ok(new { FilePath = filePath });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
