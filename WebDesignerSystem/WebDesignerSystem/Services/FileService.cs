using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace WebDesignerSystem.Services
{
    public interface IFileService
    {
        Task<string> SaveImageAsync(IFormFile imageFile, string folder = "products");
        void DeleteImage(string imageUrl);
        string GetSafeImageUrl(string imagePath);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile, string folder = "products")
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                _logger.LogWarning("Попытка сохранить пустой файл");
                return null;
            }

            try
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);

                _logger.LogInformation($"Путь сохранения: {uploadsFolder}");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    _logger.LogInformation($"Создана папка: {uploadsFolder}");
                }

                var filePath = Path.Combine(uploadsFolder, fileName);
                _logger.LogInformation($"Полный путь к файлу: {filePath}");

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                var imageUrl = $"/uploads/{folder}/{fileName}";
                _logger.LogInformation($"Изображение сохранено: {imageUrl}");
                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении изображения");
                throw new Exception($"Не удалось сохранить изображение: {ex.Message}");
            }
        }

        public void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) ||
                imageUrl.Contains("placeholder") ||
                imageUrl.StartsWith("http"))
            {
                return;
            }

            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation($"Изображение удалено: {filePath}");
                }
                else
                {
                    _logger.LogWarning($"Файл не найден: {filePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении изображения");
            }
        }

        public string GetSafeImageUrl(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return "/images/no-image.png";
            }

            if (imagePath.StartsWith("/uploads/"))
            {
                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    return imagePath;
                }
                else
                {
                    _logger.LogWarning($"Изображение не найдено: {fullPath}");
                    return "/images/no-image.png";
                }
            }

            return imagePath;
        }
    }
}