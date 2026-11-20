
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDay.Mvc.Services
{
    public class ApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpClient GetClient()
        {
            return _httpClientFactory.CreateClient("RaceDayApi");
        }

        private void ForwardApiCookie(HttpRequestMessage request)
        {
            var cookie = _httpContextAccessor.HttpContext?
                .Request.Headers.Cookie.ToString();

            if (!string.IsNullOrWhiteSpace(cookie))
            {
                request.Headers.TryAddWithoutValidation("Cookie", cookie);
            }
        }

        private void SaveApiCookies(HttpResponseMessage response)
        {
            var context = _httpContextAccessor.HttpContext;

            if (context != null &&
                response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                foreach (var cookie in cookies)
                {
                    context.Response.Headers.Append("Set-Cookie", cookie);
                }
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            ForwardApiCookie(request);

            var response = await GetClient().SendAsync(request);
            SaveApiCookies(response);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<HttpResponseMessage> PostAsync<T>(
            string endpoint, T data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(data)
            };

            ForwardApiCookie(request);

            var response = await GetClient().SendAsync(request);
            SaveApiCookies(response);

            return response;
        }

        public async Task<HttpResponseMessage> PutAsync<T>(
            string endpoint, T data)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = JsonContent.Create(data)
            };

            ForwardApiCookie(request);

            var response = await GetClient().SendAsync(request);
            SaveApiCookies(response);

            return response;
        }

        public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            ForwardApiCookie(request);

            var response = await GetClient().SendAsync(request);
            SaveApiCookies(response);

            return response;
        }

        public async Task<HttpResponseMessage> UploadFileAsync(
            string endpoint,
            IFormFile file)
        {
            using var content = new MultipartFormDataContent();

            await using var stream = file.OpenReadStream();

            using var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(file.ContentType);

            content.Add(fileContent, "file", file.FileName);

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            ForwardApiCookie(request);

            var response = await GetClient().SendAsync(request);
            SaveApiCookies(response);

            return response;
        }
    }
}
