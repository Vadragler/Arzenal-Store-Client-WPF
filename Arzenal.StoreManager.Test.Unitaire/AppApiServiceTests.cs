using Arzenal.StoreManager.Core.Interfaces;
using Arzenal.StoreManager.Core.Services;
using Arzenal.StoreManager.Core.Services.Helpers;
using Moq;
using System.Collections.ObjectModel;

namespace Arzenal.StoreManager.Test.Unitaire
{
    public class AppApiServiceTests
    {
        [Fact]
        public async Task GetAsync_DelegatesToHttpClientAndReturnsValue()
        {
            var mock = new Mock<IHttpClientService>();
            mock.Setup(m => m.CheckConnectionAsync()).ReturnsAsync(true);
            mock.Setup(m => m.GetAsync<string>("endpoint")).ReturnsAsync("value");

            var svc = new AppApiService(mock.Object);
            var result = await svc.GetAsync<string>("endpoint");

            Assert.Equal("value", result);
            mock.Verify(m => m.CheckConnectionAsync(), Times.Once);
            mock.Verify(m => m.GetAsync<string>("endpoint"), Times.Once);
        }

        [Fact]
        public async Task PostAsync_DelegatesAndReturns()
        {
            var mock = new Mock<IHttpClientService>();
            mock.Setup(m => m.CheckConnectionAsync()).ReturnsAsync(true);
            mock.Setup(m => m.PostAsync<int>("endpoint", It.IsAny<object>())).ReturnsAsync(5);

            var svc = new AppApiService(mock.Object);
            var result = await svc.PostAsync<int>("endpoint", new { });

            Assert.Equal(5, result);
            mock.Verify(m => m.CheckConnectionAsync(), Times.Once);
            mock.Verify(m => m.PostAsync<int>("endpoint", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task PutAsync_DelegatesAndReturns()
        {
            var mock = new Mock<IHttpClientService>();
            mock.Setup(m => m.CheckConnectionAsync()).ReturnsAsync(true);
            mock.Setup(m => m.PutAsync<int>("endpoint", It.IsAny<object>())).ReturnsAsync(7);

            var svc = new AppApiService(mock.Object);
            var result = await svc.PutAsync<int>("endpoint", new { });

            Assert.Equal(7, result);
            mock.Verify(m => m.CheckConnectionAsync(), Times.Once);
            mock.Verify(m => m.PutAsync<int>("endpoint", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task PatchAsync_DelegatesAndReturns()
        {
            var mock = new Mock<IHttpClientService>();
            mock.Setup(m => m.CheckConnectionAsync()).ReturnsAsync(true);
            mock.Setup(m => m.PatchAsync<int>("endpoint", It.IsAny<object>())).ReturnsAsync(9);

            var svc = new AppApiService(mock.Object);
            var result = await svc.PatchAsync<int>("endpoint", new { });

            Assert.Equal(9, result);
            mock.Verify(m => m.CheckConnectionAsync(), Times.Once);
            mock.Verify(m => m.PatchAsync<int>("endpoint", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DelegatesAndReturns()
        {
            var mock = new Mock<IHttpClientService>();
            mock.Setup(m => m.CheckConnectionAsync()).ReturnsAsync(true);
            mock.Setup(m => m.DeleteAsync("endpoint")).ReturnsAsync(true);

            var svc = new AppApiService(mock.Object);
            var result = await svc.DeleteAsync("endpoint");

            Assert.True(result);
            mock.Verify(m => m.CheckConnectionAsync(), Times.Once);
            mock.Verify(m => m.DeleteAsync("endpoint"), Times.Once);
        }

        [Fact]
        public async Task GetCollectionAsync_ReturnsObservableCollection()
        {
            var mock = new Mock<IHttpClientService>();
            mock.Setup(m => m.CheckConnectionAsync()).ReturnsAsync(true);
            mock.Setup(m => m.GetAsync<string[]>("endpoint")).ReturnsAsync(new[] { "a", "b" });

            var svc = new AppApiService(mock.Object);
            var result = await svc.GetCollectionAsync<string>("endpoint");

            Assert.IsType<ObservableCollection<string>>(result);
            Assert.Equal(2, result.Count);
            mock.Verify(m => m.CheckConnectionAsync(), Times.Once);
            mock.Verify(m => m.GetAsync<string[]>("endpoint"), Times.Once);
        }

        [Fact]
        public async Task PostWithFullResponseAsync_SetsTokenAndCalls()
        {
            var mock = new Mock<IHttpClientService>();
            mock.Setup(m => m.PostWithFullResponseAsync<string>("endpoint", It.IsAny<object>()))
                .ReturnsAsync(new HttpClientService.HttpResult<string> { Body = "x" });

            var svc = new AppApiService(mock.Object);
            var res = await svc.PostWithFullResponseAsync<string>("endpoint", new { }, "token123");

            Assert.Equal("x", res.Body);
            mock.Verify(m => m.SetBearerToken("token123"), Times.Once);
            mock.Verify(m => m.PostWithFullResponseAsync<string>("endpoint", It.IsAny<object>()), Times.Once);
        }
    }
}
