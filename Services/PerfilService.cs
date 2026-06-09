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
            perfil.PuntosDigitales = int.Parse(perfilApi.puntosDigitales);
            perfil.Departamento = perfilApi.departamento;
            perfil.Ubicacion = perfilApi.ubicacion;

            perfil.ResumenProfesional = "Profesional con experiencia en la gestion de proyectos y coordinacion de equipos.";

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

                articulo.Icono = "gift";
                articulo.Nombre = articuloApi.nombre;
                articulo.Rareza = articuloApi.rareza;
                articulo.EsUltraRaro = articuloApi.rareza == "Ultra Raro";

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

   public class PerfilApiResponse
    {
        public int idUsuario { get; set; }
        public string nombre { get; set; }
        public string puesto { get; set; }
        public string nivel { get; set; }
        public string puntosDigitales { get; set; }
        public string departamento { get; set; }
        public string ubicacion { get; set; }
        public string? fotoDePerfil { get; set; }

        public List<LogroApi> logros { get; set; }
        public List<ArticuloApi> articulosVirtuales { get; set; }
    }

    public class LogroApi
    {
        public int IDLogro { get; set; }
        public string Nombre { get; set; }
        public int Kilometros { get; set; }
        public int Medallones { get; set; }
        public int Nitro { get; set; }
    }

    public class ArticuloApi
    {
        public int id { get; set; }
        public string tipo { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public int precio { get; set; }
        public string rareza { get; set; }
        public bool equipado { get; set; }
    }
}