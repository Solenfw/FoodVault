namespace FoodVault.Services
{
    public class FileUploadService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileUploadService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string subfolder)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            // The path to the wwwroot folder
            var wwwRootPath = _webHostEnvironment.WebRootPath;

            // The path to the subfolder (e.g., wwwroot/uploads/avatars)
            var uploadsFolder = Path.Combine(wwwRootPath, "uploads", subfolder);

            // Create the directory if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Create a unique file name to avoid conflicts
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the web-accessible path to be stored in the database
            return $"/uploads/{subfolder}/{uniqueFileName}";
        }
    }
}