using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Arzenal.StoreManager.Core.Services;
using Arzenal.StoreManager.Core.Dtos;
using Arzenal.Dto.DTOs.FileDto;
using Arzenal.StoreManager.Core.Services.Helpers;
using Xunit;

namespace Arzenal.StoreManager.Test.Unitaire
{
    public class HttpClientServiceTests
    {
        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

            public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
            {
                _responder = responder ?? throw new ArgumentNullException(nameof(responder));
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_responder(request));
            }
        }

        private record TestDto(string? Value);
        private record ResultDto(bool Result);

        [Fact]
        public void Constructor_Sets_DefaultHeaders()
        {
            var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new HttpClientService(client);

            Assert.True(client.DefaultRequestHeaders.Contains("X-Device-Name"));
            Assert.True(client.DefaultRequestHeaders.Contains("Fingerprint"));
            Assert.True(client.DefaultRequestHeaders.UserAgent.ToString().Contains("ArzenalStoreManager"));
        }

        [Fact]
        public void SetBearerToken_Sets_AuthorizationHeader()
        {
            var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new HttpClientService(client);

            service.SetBearerToken("abc123");

            Assert.NotNull(client.DefaultRequestHeaders.Authorization);
            Assert.Equal("Bearer", client.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal("abc123", client.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public async Task GetAsync_Returns_DeserializedObject_OnJson()
        {
            var dto = new TestDto("hello");
            var json = JsonSerializer.Serialize(dto);
            var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new HttpClientService(client);

            var result = await service.GetAsync<TestDto>("api/test");

            Assert.NotNull(result);
            Assert.Equal("hello", result?.Value);
        }

        [Fact]
        public async Task GetAsync_Returns_Stream_ForStreamType()
        {
            var bytes = Encoding.UTF8.GetBytes("filecontent");
            var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            });

            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new HttpClientService(client);

            var stream = await service.GetAsync<Stream>("api/file");

            Assert.NotNull(stream);
            using var ms = new MemoryStream();
            await ((Stream)stream!).CopyToAsync(ms);
            Assert.Equal(bytes.Length, ms.Length);
        }

        [Fact]
        public async Task GetAsync_Returns_Default_OnEmptyBody()
        {
            var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
            });

            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new HttpClientService(client);

            var result = await service.GetAsync<TestDto>("api/empty");

            Assert.Null(result);
        }

        [Fact]
        public async Task PostAsync_With_File_Upload_PostsCorrectEndpoint_AndReturnsDeserialized()
        {
            UploadFileClientDto captured = null!;
            HttpRequestMessage lastRequest = null!;

            // create temp file
            var tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, "dummy");

            var uploadDto = new UploadFileClientDto
            {
                LocalPath = tempFile,
                ApiDto = new UploadFileDto
                {
                    AppId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Version = "1.2.3",
                    Type = "app",
                    FileName = "myfile.bin",
                    Platform = "Windows"
                }
            };

            var responseObj = new ResultDto(true);
            var responseJson = JsonSerializer.Serialize(responseObj);

            var handler = new TestHttpMessageHandler(req =>
            {
                lastRequest = req;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                };
            });

            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new HttpClientService(client);

            var result = await service.PostAsync<ResultDto>("unused", uploadDto);

            Assert.NotNull(result);
            Assert.True(result?.Result);
            Assert.NotNull(lastRequest);
            Assert.Equal(HttpMethod.Post, lastRequest.Method);
            // verify constructed path contains AppId, version, type and filename
            var path = lastRequest.RequestUri!.AbsolutePath.ToLowerInvariant();
            Assert.Contains("apps/11111111-1111-1111-1111-111111111111", path);
            Assert.Contains("versions/1.2.3", path);
            Assert.Contains("files/app", path);
            Assert.Contains("filename/myfile.bin", path);

            File.Delete(tempFile);
        }

        [Fact]
        public async Task PostWithFullResponseAsync_Returns_HttpResult_With_Body()
        {
            var obj = new TestDto("ok");
            var json = JsonSerializer.Serialize(obj);
            var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new HttpClientService(client);

            var result = await service.PostWithFullResponseAsync<TestDto>("api/x", new { a = 1 });

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Body);
            Assert.Equal("ok", result.Body?.Value);
        }

        [Fact]
        public async Task PutAsync_Returns_Deserialized()
        {
            var dto = new TestDto("put");
            var json = JsonSerializer.Serialize(dto);
            var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new HttpClientService(client);

            var result = await service.PutAsync<TestDto>("api/put", new { });

            Assert.NotNull(result);
            Assert.Equal("put", result?.Value);
        }

        [Fact]
        public async Task PatchAsync_Sends_Patch_Method_And_Returns_Deserialized()
        {
            var dto = new TestDto("patch");
            var json = JsonSerializer.Serialize(dto);
            HttpRequestMessage? last = null;
            var handler = new TestHttpMessageHandler(req =>
            {
                last = req;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new HttpClientService(client);

            var result = await service.PatchAsync<TestDto>("api/patch", new { });

            Assert.NotNull(result);
            Assert.Equal("patch", result?.Value);
            Assert.NotNull(last);
            Assert.Equal("PATCH", last.Method.Method);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrueOnSuccess()
        {
            var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.NoContent));
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new HttpClientService(client);

            var ok = await service.DeleteAsync("api/del");
            Assert.True(ok);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalseOnFailure()
        {
            var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.BadRequest));
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new HttpClientService(client);

            var ok = await service.DeleteAsync("api/del");
            Assert.False(ok);
        }

        [Fact]
        public async Task CheckConnectionAsync_PingSuccess_ReturnsTrueAndSetsConnected()
        {
            // debug -> 200, ping -> 200
            var handler = new TestHttpMessageHandler(req =>
            {
                var path = req.RequestUri!.AbsolutePath.ToLowerInvariant();
                if (path.Contains("/api/health/debug/claims"))
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("claims") };
                if (path.Contains("/api/auth/ping"))
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("pong") };
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new HttpClientService(client);

            AppSessionState.Instance.IsConnected = false;
            var res = await service.CheckConnectionAsync();

            Assert.True(res);
            Assert.True(AppSessionState.Instance.IsConnected);
        }

        [Fact]
        public async Task CheckConnectionAsync_RefreshFlow_Works_When_PingInitiallyFails()
        {
            // debug -> 200, ping -> 401 first, refresh -> 200, ping -> 200
            int pingCount = 0;
            var handler = new TestHttpMessageHandler(req =>
            {
                var path = req.RequestUri!.AbsolutePath.ToLowerInvariant();
                if (path.Contains("/api/health/debug/claims"))
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("claims") };
                if (path.Contains("/api/auth/ping"))
                {
                    pingCount++;
                    if (pingCount == 1) return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("pong") };
                }
                if (path.Contains("/api/auth/refresh"))
                    return new HttpResponseMessage(HttpStatusCode.OK);
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new HttpClientService(client);

            AppSessionState.Instance.IsConnected = false;
            var res = await service.CheckConnectionAsync();

            Assert.True(res);
            Assert.True(AppSessionState.Instance.IsConnected);
        }

        [Fact]
        public async Task CheckConnectionAsync_ReturnsFalse_When_AllFail()
        {
            var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new HttpClientService(client);

            AppSessionState.Instance.IsConnected = true;
            var res = await service.CheckConnectionAsync();

            Assert.False(res);
            Assert.False(AppSessionState.Instance.IsConnected);
        }

        [Fact]
        public async Task HandleHttpError_Sets_AppSessionState_False_On_Unauthorized()
        {
            // any request returning 401 should set AppSessionState false via HttpErrorService
            var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.Unauthorized));
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            var service = new HttpClientService(client);

            AppSessionState.Instance.IsConnected = true;
            var result = await service.GetAsync<TestDto>("api/secured");

            Assert.False(AppSessionState.Instance.IsConnected);
            Assert.Null(result);
        }
    }
}
