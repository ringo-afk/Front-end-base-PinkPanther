using System.Text.Json;
using PinkPanther.Models;

namespace PinkPanther.Services
{
    public class TiendaService : ITiendaService
    {
        private readonly HttpClient _httpClient;

        public TiendaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ObjetoTienda>> ObtenerCatalogo()
        {
            
            var url = "https://10.14.255.40:8003/api/tienda/accesorios";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return new List<ObjetoTienda>();

            
            var listaDatos = await response.Content.ReadFromJsonAsync<List<ObjetoTienda>>();
            return listaDatos ?? new List<ObjetoTienda>();
        }
    }
}