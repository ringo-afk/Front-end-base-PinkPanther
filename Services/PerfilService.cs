using System.Text.Json;
using PinkPanther.Models;

namespace PinkPanther.Services
{
    public class PerfilService : IPerfilService
    {
        private readonly HttpClient _httpClient;

        public PerfilService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> GuardarResumen(int idUsuario, string resumenProfesional)
        {
            var url = "https://10.14.255.40:8002/api/perfil/" + idUsuario + "/resumen";

            var datos = new
            {
                resumenProfesional = resumenProfesional
            };

            var json = JsonSerializer.Serialize(datos);

            var content = new StringContent(
                json,
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, content);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool?> EquiparAccesorio(int idUsuario, int idAccesorio)
        {
            var url = "https://10.14.255.40:8000/api/accesorios/toggle-equipar";

            var datos = new
            {
                usuario_id = idUsuario,
                accesorio_id = idAccesorio
            };

            var json = JsonSerializer.Serialize(datos);
            var content = new StringContent(
                json,
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonResult = await response.Content.ReadAsStringAsync();
            
            using (var doc = JsonDocument.Parse(jsonResult))
            {
                return doc.RootElement.GetProperty("equipado").GetBoolean();
            }
        }

        public async Task<PerfilViewModel?> ObtenerPerfil(int idUsuario)
        {
            var url = "https://10.14.255.40:8002/api/perfil/" + idUsuario;

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            var opciones = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var perfilApi = JsonSerializer.Deserialize<PerfilApiResponse>(json, opciones);

            if (perfilApi == null)
            {
                return null;
            }

            PerfilViewModel perfil = new PerfilViewModel();

            perfil.Nombre = perfilApi.nombre;
            perfil.Puesto = perfilApi.puesto;
            perfil.Nivel = perfilApi.nivel;

            int puntos = 0;
            int.TryParse(perfilApi.puntosDigitales, out puntos);
            perfil.PuntosDigitales = puntos;

            perfil.Departamento = perfilApi.departamento;
            perfil.Ubicacion = perfilApi.ubicacion;
            perfil.ResumenProfesional = perfilApi.resumenProfesional;

            perfil.Logros = new List<Logro>();

            foreach (var logroApi in perfilApi.logros)
            {
                Logro logro = new Logro();

                logro.Icono = "star";
                logro.Titulo = logroApi.Nombre;
                logro.Descripcion = "Kilómetros: " + logroApi.Kilometros + " | Medallones: " + logroApi.Medallones + " | Nitro: " + logroApi.Nitro;

                perfil.Logros.Add(logro);
            }

            perfil.ArticulosVirtuales = new List<ArticuloVirtual>();

            foreach (var articuloApi in perfilApi.articulosVirtuales)
            {
                ArticuloVirtual articulo = new ArticuloVirtual();

                articulo.Id = articuloApi.id;
                articulo.Tipo = articuloApi.tipo;
                articulo.Icono = "gift";
                articulo.Nombre = articuloApi.nombre;
                articulo.Rareza = articuloApi.rareza;
                articulo.EsUltraRaro = articuloApi.rareza == "Ultra Raro";
                articulo.Equipado = articuloApi.equipado;

                perfil.ArticulosVirtuales.Add(articulo);
            }

            perfil.Habilidades = new List<Habilidad>()
            {
                new Habilidad { Nombre = "Liderazgo" },
                new Habilidad { Nombre = "Gestión" },
                new Habilidad { Nombre = "Resolución" }
            };

            return perfil;
        }
    }
}