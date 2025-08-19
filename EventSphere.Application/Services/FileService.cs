using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface IFileService
    {
        Task<string?> SaveUserProfileImageAsync(int userId, IFormFile profileImage);
    }

    public class FileService : IFileService
    {
        public async Task<string?> SaveUserProfileImageAsync(int userId, IFormFile profileImage)
        {
            if (profileImage == null || profileImage.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);
            var fileName = $"user_{userId}_" + Guid.NewGuid().ToString("N") + Path.GetExtension(profileImage.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }
            return $"/uploads/profile/{fileName}";
        }
    }
}
