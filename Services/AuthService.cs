using System.Text;
using System.Text.Json;

namespace PinkPanther.Services
{
    public interface IAuthService
    {
        Task<PythonApiResponse?> AuthenticateAsync(string email, string password);
    }

    public class PythonApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Kilometros { get; set; }
    }

    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PythonApiResponse?> AuthenticateAsync(string email, string password)
        {
            var client = _httpClientFactory.CreateClient("PythonApi");
            var pythonApiUrl = "https://127.0.0.1:8000/api/login";

            var payload = new { email, password };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(pythonApiUrl, jsonContent);
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<PythonApiResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}