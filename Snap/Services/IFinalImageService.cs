using Snap.Models;

namespace Snap.Services
{
    public interface IFinalImageService
    {
        Task<string> GenerateFinalImageAsync(FinalImageRequest request, string userId);
    }
}