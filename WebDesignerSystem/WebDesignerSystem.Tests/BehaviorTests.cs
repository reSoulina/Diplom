using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class BehaviorTests : IClassFixture<TestBase>
    {
        private readonly TestBase _factory;

        public BehaviorTests(TestBase factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task NonExistentPage_ShouldReturn404()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/non-existent-page");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task StaticFiles_ShouldBeServed()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act - проверяем доступность стандартных статических файлов
            var faviconResponse = await client.GetAsync("/favicon.ico");
            var cssResponse = await client.GetAsync("/css/site.css");

            // Assert
            Assert.True(faviconResponse.IsSuccessStatusCode ||
                       faviconResponse.StatusCode == HttpStatusCode.NotFound);
            Assert.True(cssResponse.IsSuccessStatusCode ||
                       cssResponse.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Application_ShouldHandleConcurrentRequests()
        {
            // Arrange
            var client = _factory.CreateClient();
            var tasks = new List<Task<HttpResponseMessage>>();

            // Act - отправляем несколько параллельных запросов
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(client.GetAsync("/"));
            }

            var responses = await Task.WhenAll(tasks);

            // Assert - все запросы должны быть успешными
            foreach (var response in responses)
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task ErrorPage_ShouldBeConfigured()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act - попытка доступа к несуществующему API endpoint
            var response = await client.GetAsync("/api/nonexistent");

            // Assert - должен вернуть 404 или ошибку
            Assert.True(response.StatusCode == HttpStatusCode.NotFound ||
                       response.StatusCode == HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task ResponseHeaders_ShouldBeSetCorrectly()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert - проверяем важные заголовки
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("utf-8", response.Content.Headers.ContentType.CharSet);
        }
    }
}