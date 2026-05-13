using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using WebDesignerSystem.Services;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class FileServiceTests : IDisposable
    {
        private readonly string _testWebRootPath;
        private readonly Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment> _environmentMock;
        private readonly Mock<ILogger<FileService>> _loggerMock;
        private readonly FileService _fileService;

        public FileServiceTests()
        {
            _testWebRootPath = Path.Combine(Path.GetTempPath(), "WebDesignerSystem_Test");
            if (Directory.Exists(_testWebRootPath))
            {
                Directory.Delete(_testWebRootPath, true);
            }
            Directory.CreateDirectory(_testWebRootPath);

            _environmentMock = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            _environmentMock.Setup(x => x.WebRootPath).Returns(_testWebRootPath);

            _loggerMock = new Mock<ILogger<FileService>>();
            _fileService = new FileService(_environmentMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task SaveImageAsync_ShouldSaveFileAndReturnUrl()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var fileName = "test-image.jpg";
            var content = "Fake image content";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream, token));

            // Act
            var result = await _fileService.SaveImageAsync(fileMock.Object, "products");

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("/uploads/products/", result);
            Assert.EndsWith(".jpg", result);

            var savedFilePath = Path.Combine(_testWebRootPath, result.TrimStart('/'));
            Assert.True(File.Exists(savedFilePath));
        }

        [Fact]
        public async Task SaveImageAsync_WhenFileIsNull_ShouldReturnNull()
        {
            // Act
            var result = await _fileService.SaveImageAsync(null, "products");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SaveImageAsync_WhenFileIsEmpty_ShouldReturnNull()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);

            // Act
            var result = await _fileService.SaveImageAsync(fileMock.Object, "products");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DeleteImage_WhenFileExists_ShouldDelete()
        {
            // Arrange
            var testFile = Path.Combine(_testWebRootPath, "uploads", "products", "test-delete.jpg");
            Directory.CreateDirectory(Path.GetDirectoryName(testFile));
            File.WriteAllText(testFile, "test content");

            var imageUrl = $"/uploads/products/test-delete.jpg";

            // Act
            _fileService.DeleteImage(imageUrl);

            // Assert
            Assert.False(File.Exists(testFile));
        }

        [Fact]
        public void DeleteImage_WhenFileNotExists_ShouldNotThrow()
        {
            // Arrange
            var imageUrl = "/uploads/products/non-existent-file.jpg";

            // Act & Assert
            var exception = Record.Exception(() => _fileService.DeleteImage(imageUrl));
            Assert.Null(exception);
        }

        [Fact]
        public void DeleteImage_WhenUrlIsPlaceholder_ShouldNotDelete()
        {
            // Act
            var exception = Record.Exception(() => _fileService.DeleteImage("https://via.placeholder.com/400x300"));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void GetSafeImageUrl_WhenUrlIsNull_ShouldReturnNoImage()
        {
            // Act
            var result = _fileService.GetSafeImageUrl(null);

            // Assert
            Assert.Equal("/images/no-image.png", result);
        }

        [Fact]
        public void GetSafeImageUrl_WhenUrlIsEmpty_ShouldReturnNoImage()
        {
            // Act
            var result = _fileService.GetSafeImageUrl("");

            // Assert
            Assert.Equal("/images/no-image.png", result);
        }

        [Fact]
        public void GetSafeImageUrl_WhenFileExists_ShouldReturnOriginalUrl()
        {
            // Arrange
            var testFile = Path.Combine(_testWebRootPath, "uploads", "products", "exists.jpg");
            Directory.CreateDirectory(Path.GetDirectoryName(testFile));
            File.WriteAllText(testFile, "test");

            var imageUrl = "/uploads/products/exists.jpg";

            // Act
            var result = _fileService.GetSafeImageUrl(imageUrl);

            // Assert
            Assert.Equal(imageUrl, result);
        }

        [Fact]
        public void GetSafeImageUrl_WhenFileNotExists_ShouldReturnNoImage()
        {
            // Act
            var result = _fileService.GetSafeImageUrl("/uploads/products/not-exists.jpg");

            // Assert
            Assert.Equal("/images/no-image.png", result);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testWebRootPath))
            {
                Directory.Delete(_testWebRootPath, true);
            }
        }
    }
}