namespace FoodVault.Services.Interfaces;

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;

public interface IImageService {
    /// <summary>
    /// Uploads the image file and returns the URL/path saved.
    /// </summary>
    Task<string> UploadImageAsync(IFormFile file, string folder = "images");

    /// <summary>
    /// Resizes the given image bytes and returns the new bytes.
    /// </summary>
    Task<byte[]> ResizeImageAsync(byte[] imageBytes, int width, int height);

    /// <summary>
    /// Deletes image by path or url.
    /// </summary>
    Task<bool> DeleteImageAsync(string imagePath);
    

    /// <summary>
    /// Gets accessible URL for an image file.
    /// </summary>
    string GetImageUrl(string imagePath);

    /// <summary>
    /// Returns a stream for serving the image.
    /// </summary>
    Task<Stream> GetImageStreamAsync(string imagePath);
}
