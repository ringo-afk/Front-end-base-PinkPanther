using System.Text.Json;
using PinkPanther.Models;
using Microsoft.Extensions.Http;
namespace PinkPanther.Services;

public class UsuarioService : IUsuarioService
    {
        private readonly HttpClient _httpClient;

        public UsuarioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Usuario>> ObtenerUsuarios()
        {
            var url = "https://192.168.1.16:8443/Usuario_Km";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new List<Usuario>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Usuario>>(json); 
        }
    }