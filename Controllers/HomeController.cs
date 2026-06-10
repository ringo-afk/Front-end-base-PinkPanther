using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PinkPanther.Models;
using PinkPanther.Services;
using System.Text.Json;

namespace PinkPanther.Controllers
{
    public class HomeController : Controller
    {
        private static readonly List<ObjetoTienda> CatalogoBase = new List<ObjetoTienda>
        {
            new ObjetoTienda { Id = 1, Nombre = "Chamarra Élite", Categoria = "Avatar", CostoPuntos = 1500, RutaImagen = "~/Imagenes/Chamarra Elite.png" },
            new ObjetoTienda { Id = 2, Nombre = "Lentes VR de Neón", Categoria = "Avatar", CostoPuntos = 1500, RutaImagen = "~/Imagenes/Lentes VR.png" },
            new ObjetoTienda { Id = 3, Nombre = "Rines Carro", Categoria = "Accesorio Auto", CostoPuntos = 1500, RutaImagen = "~/Imagenes/Rines-carro.png" },
            new ObjetoTienda { Id = 4, Nombre = "Carro Moderno", Categoria = "Auto", CostoPuntos = 2500, RutaImagen = "~/Imagenes/Carro-moderno.png" },
            new ObjetoTienda { Id = 5, Nombre = "Gorra Negra", Categoria = "Avatar", CostoPuntos = 1500, RutaImagen = "~/Imagenes/Gorra-negra.png" },
            new ObjetoTienda { Id = 6, Nombre = "Casco Ingeniero", Categoria = "Avatar", CostoPuntos = 2500, RutaImagen = "~/Imagenes/Casco-Ingeniero.png" },
            new ObjetoTienda { Id = 7, Nombre = "Objeto Aleatorio", Categoria = "Global", CostoPuntos = 1000, RutaImagen = "~/Imagenes/Random.png" },
            new ObjetoTienda { Id = 8, Nombre = "Carro Deportivo", Categoria = "Auto", CostoPuntos = 6000, RutaImagen = "~/Imagenes/Carro-deportivo.png" }
        };

        private static int? ObjetoEquipadoId = 6; 

        private readonly IAuthService _authService;
        private readonly IUsuarioService _usuarioService;
        private readonly ICatalogoService _catalogoService;

        public HomeController(IAuthService authService, IUsuarioService usuarioService, ICatalogoService catalogoService)
        {
            _authService = authService;
            _usuarioService = usuarioService;
            _catalogoService = catalogoService;
        }

        private UsuarioJuego ObtenerUsuarioActual()
        {
            var nombre = HttpContext.Session.GetString("NombreUsuario");
            var puntos = HttpContext.Session.GetInt32("PuntosUsuario");

            if (!string.IsNullOrEmpty(nombre))
            {
                return new UsuarioJuego 
                { 
                    Nombre = nombre, 
                    Rol = "Jugador", 
                    PuntosDisponibles = puntos ?? 0 
                };
            }
            return null;
        }

        private List<int> ObtenerObjetosAdquiridos()
        {
            var adquiridosJson = HttpContext.Session.GetString("ObjetosAdquiridos");
            if (string.IsNullOrEmpty(adquiridosJson))
            {
                var adquiridos = new List<int> { 3 };
                GuardarObjetosAdquiridos(adquiridos);
                return adquiridos;
            }
            return JsonSerializer.Deserialize<List<int>>(adquiridosJson)!;
        }

        private void GuardarObjetosAdquiridos(List<int> adquiridos)
        {
            HttpContext.Session.SetString("ObjetosAdquiridos", JsonSerializer.Serialize(adquiridos));
        }

        private void CargarDatosPanelUsuario()
        {
            var usuarioActual = ObtenerUsuarioActual();
            if(usuarioActual != null)
            {
                ViewData["NombreUsuario"] = usuarioActual.Nombre;
                ViewData["RolUsuario"] = usuarioActual.Rol;
                ViewData["PuntosUsuario"] = usuarioActual.PuntosDisponibles.ToString("N0");
            }
        }

        public IActionResult Index()
        {
            if (ObtenerUsuarioActual() == null) return RedirectToAction("Login", "Home");

            CargarDatosPanelUsuario();
            return View();
        }

        public IActionResult Privacy()
        {
            if (ObtenerUsuarioActual() == null) return RedirectToAction("Login", "Home");

            CargarDatosPanelUsuario();
            return View();
        }

        public async Task<IActionResult> Catalogo(string dificultad)
        {
            if (ObtenerUsuarioActual() == null) return RedirectToAction("Login", "Home");

            CargarDatosPanelUsuario();

            var juegos = string.IsNullOrEmpty(dificultad)
                ? await _catalogoService.ObtenerJuegosAsync()
                : await _catalogoService.ObtenerJuegosPorDificultadAsync(dificultad);

            var model = new CatalogoJuegosViewModel
            {
                Juegos = juegos,
                DificultadSeleccionada = dificultad
            };

            return View(model);
        }

        public IActionResult Tienda()
        {
            if (ObtenerUsuarioActual() == null) return RedirectToAction("Login", "Home");

            CargarDatosPanelUsuario();
            var model = ConstruirTiendaViewModel();
            return View(model);
        }

        public async Task<IActionResult> TablaClasificatoria()
        {
            if (ObtenerUsuarioActual() == null) return RedirectToAction("Login", "Home");

            try
            {
                var usuarios = await _usuarioService.ObtenerUsuarios();
                return View(usuarios);
            }
            catch
            {
                return View(new List<Usuario>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Comprar(int objetoId)
        {
            var usuarioActual = ObtenerUsuarioActual();
            if (usuarioActual == null) return RedirectToAction("Login", "Home");

            ObjetoTienda? objeto = CatalogoBase.Find(item => item.Id == objetoId);

            if (objeto == null)
            {
                TempData["CompraError"] = "No se encontró el objeto seleccionado.";
                return RedirectToAction("Tienda");
            }

            var objetosAdquiridos = ObtenerObjetosAdquiridos();

            if (objetosAdquiridos.Contains(objeto.Id) || ObjetoEquipadoId == objeto.Id)
            {
                TempData["CompraError"] = objeto.Nombre + " ya forma parte de tu inventario.";
                return RedirectToAction("Tienda");
            }

            if (objeto.CostoPuntos > usuarioActual.PuntosDisponibles)
            {
                TempData["CompraError"] = "No tienes puntos suficientes para comprar " + objeto.Nombre + ".";
                return RedirectToAction("Tienda");
            }

            usuarioActual.PuntosDisponibles -= objeto.CostoPuntos;
            HttpContext.Session.SetInt32("PuntosUsuario", usuarioActual.PuntosDisponibles);

            objetosAdquiridos.Add(objeto.Id);
            GuardarObjetosAdquiridos(objetosAdquiridos);

            TempData["CompraExitosa"] = "Compra realizada: " + objeto.Nombre + " por " + objeto.CostoPuntos.ToString("N0") + " puntos.";
            return RedirectToAction(nameof(Tienda));
        }


        [HttpGet]
        [Route("Login")]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("NombreUsuario") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            
            return View("~/Views/Home/Login.cshtml");
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Home/Login.cshtml", model);
            }

            var resultadoLogin = await _authService.AuthenticateAsync(model.Email, model.Password);

            if (resultadoLogin != null && resultadoLogin.Success)
            {
                HttpContext.Session.SetString("NombreUsuario", resultadoLogin.Nombre);
                HttpContext.Session.SetInt32("PuntosUsuario", resultadoLogin.Kilometros);
                HttpContext.Session.SetInt32("IdUsuario", resultadoLogin.IdUsuario);
                
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, resultadoLogin?.Message ?? "Correo o contraseña incorrectos.");
            
            return View("~/Views/Home/Login.cshtml", model);
        }

        private TiendaViewModel ConstruirTiendaViewModel()
        {
            var objetos = new List<ObjetoTienda>();
            var objetosAdquiridos = ObtenerObjetosAdquiridos();
            var usuarioActual = ObtenerUsuarioActual();

            foreach (var item in CatalogoBase)
            {
                var estado = EstadoObjetoTienda.Disponible;

                if (ObjetoEquipadoId == item.Id)
                {
                    estado = EstadoObjetoTienda.Equipado;
                }
                else if (objetosAdquiridos.Contains(item.Id))
                {
                    estado = EstadoObjetoTienda.Adquirido;
                }
                else if (item.CostoPuntos > usuarioActual.PuntosDisponibles)
                {
                    estado = EstadoObjetoTienda.Bloqueado;
                }

                var obj = new ObjetoTienda
                {
                    Id = item.Id,
                    Nombre = item.Nombre,
                    Categoria = item.Categoria,
                    CostoPuntos = item.CostoPuntos,
                    RutaImagen = item.RutaImagen,
                    TextoConfirmacion = ObtenerTextoConfirmacion(item.Nombre),
                    Estado = estado,
                    EsComprable = (estado == EstadoObjetoTienda.Disponible)
                };

                objetos.Add(obj);
            }

            var model = new TiendaViewModel
            {
                Usuario = usuarioActual, 
                Objetos = objetos
            };

            return model;
        }

        private static string ObtenerTextoConfirmacion(string nombreObjeto)
        {
            if (nombreObjeto == "Chamarra Élite") return "la Chamarra Élite";
            if (nombreObjeto == "Lentes VR de Neón") return "los Lentes VR de Neón";
            if (nombreObjeto == "Rines Carro") return "los Rines Carro";
            if (nombreObjeto == "Carro Moderno") return "el Carro Moderno";
            if (nombreObjeto == "Gorra Negra") return "la Gorra Negra";
            if (nombreObjeto == "Casco Ingeniero") return "el Casco Ingeniero";
            if (nombreObjeto == "Objeto Aleatorio") return "el Objeto Aleatorio";
            if (nombreObjeto == "Carro Deportivo") return "el Carro Deportivo";
            return nombreObjeto;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}