using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace SchoolManagement.Services
{
    public interface IFileService
    {
        Task<string?> UploadFileAsync(IFormFile? file, string targetSubfolder, string[]? allowedExtensions = null, long maxSizeBytes = 5242880);
        void DeleteFile(string? relativeFilePath);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private static readonly string[] DefaultAllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> UploadFileAsync(
            IFormFile? file,
            string targetSubfolder,
            string[]? allowedExtensions = null,
            long maxSizeBytes = 5242880) // 5MB default
        {
            if (file == null || file.Length == 0)
                return null;

            if (file.Length > maxSizeBytes)
                throw new InvalidOperationException($"File size exceeds the allowed limit of {maxSizeBytes / (1024 * 1024)}MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var validExtensions = allowedExtensions ?? DefaultAllowedExtensions;

            if (!validExtensions.Contains(ext))
                throw new InvalidOperationException($"Invalid file extension. Allowed extensions: {string.Join(", ", validExtensions)}");

            // WebRoot directory
            var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsDirectory = Path.Combine(webRoot, "uploads", targetSubfolder);

            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            // Secure unique file name
            var uniqueFileName = $"{Guid.NewGuid():N}_{DateTime.UtcNow.Ticks}{ext}";
            var fullPath = Path.Combine(uploadsDirectory, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative web URL
            return $"/uploads/{targetSubfolder}/{uniqueFileName}";
        }

        public void DeleteFile(string? relativeFilePath)
        {
            if (string.IsNullOrWhiteSpace(relativeFilePath))
                return;

            try
            {
                var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var trimmed = relativeFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(webRoot, trimmed);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
                // Silently ignore delete errors in cleanup
            }
        }
    }
}
