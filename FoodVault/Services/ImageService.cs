using System.IO;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace FoodVault.Services;

public sealed class ImageService : IImageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ImageService> _logger;

    public ImageService(IWebHostEnvironment env, ILogger<ImageService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder = "images")
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty", nameof(file));
        }
        try
        {
            var uploadsRoot = Path.Combine(_env.WebRootPath, folder);
            if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);
            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsRoot, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var relative = $"/{folder.Trim('/')}/{fileName}";
            return relative;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save image to folder {Folder}", folder);
            throw;
        }
    }

    // Resize thật dùng SixLabors.ImageSharp
    public async Task<byte[]> ResizeImageAsync(byte[] imageBytes, int width, int height)
    {
        try {
            using var inStream = new MemoryStream(imageBytes);
            using var image = await Image.LoadAsync(inStream);
            image.Mutate(x => x.Resize(width, height));
            await using var outStream = new MemoryStream();
            await image.SaveAsJpegAsync(outStream);
            return outStream.ToArray();
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to resize image");
            throw;
        }
    }

    public async Task<bool> DeleteImageAsync(string imagePath)
    {
        try {
            var fullFile = imagePath.StartsWith("") && !imagePath.StartsWith("/")
                ? Path.Combine(_env.WebRootPath, imagePath)
                : _env.WebRootPath + imagePath.Replace("/", Path.DirectorySeparatorChar.ToString());
            if (File.Exists(fullFile)) {
                File.Delete(fullFile);
                return true;
            }
            return false;
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to delete image {ImagePath}", imagePath);
            return false;
        }
    }

    public string GetImageUrl(string imagePath)
    {
        if (imagePath.StartsWith("/")) return imagePath;
        return $"/images/{imagePath}";
    }

    public async Task<Stream> GetImageStreamAsync(string imagePath)
    {
        var fullFile = _env.WebRootPath + imagePath.Replace("/", Path.DirectorySeparatorChar.ToString());
        return File.Exists(fullFile) ? new FileStream(fullFile, FileMode.Open, FileAccess.Read) : Stream.Null;
    }
}









