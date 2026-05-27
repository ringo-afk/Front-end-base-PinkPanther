using System.Text.Json;
using PinkPanther.Models;
namespace PinkPanther.Services;

public class CatalogoService : ICatalogoService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CatalogoService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<Juego>> ObtenerJuegosAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("CatalogoApi");
            var url = "https://127.0.0.1:8001/juegos";

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return new List<Juego>();

            return await response.Content.ReadFromJsonAsync<List<Juego>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<Juego>();
        }
        catch (Exception)
        {
            return new List<Juego>();
           
        }
    }

    public async Task<List<Juego>> ObtenerJuegosPorDificultadAsync(string dificultad)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("CatalogoApi");
            var url = $"https://127.0.0.1:8001/juegos/dificultad/{dificultad}";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return new List<Juego>();

            return await response.Content.ReadFromJsonAsync<List<Juego>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<Juego>();
        }
        catch (Exception)
        {
            return new List<Juego>();

        }
    }
}
