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
        
        private static int? ObjetoEquipadoId = null; 

        private readonly IAuthService _authService;
        private readonly IUsuarioService _usuarioService;
        private readonly ICatalogoService _catalogoService;
        private readonly ITiendaService _tiendaService; 

        
        public HomeController(IAuthService authService, IUsuarioService usuarioService, ICatalogoService catalogoService, ITiendaService tiendaService)
        {
            _authService = authService;
            _usuarioService = usuarioService;
            _catalogoService = catalogoService;
            _tiendaService = tiendaService;
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
                
                var adquiridos = new List<int>(); 
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

        
        public IActionResult Juego()
        {
            if (ObtenerUsuarioActual() == null) return RedirectToAction("Login", "Home");

            CargarDatosPanelUsuario();
            
            return View("~/Views/Home/Juego.cshtml");
        }

        
        public async Task<IActionResult> Tienda()
        {
            if (ObtenerUsuarioActual() == null) return RedirectToAction("Login", "Home");

            CargarDatosPanelUsuario();
            var model = await ConstruirTiendaViewModelAsync();
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
        public async Task<IActionResult> Comprar(int objetoId)
        {
            var usuarioActual = ObtenerUsuarioActual();
            if (usuarioActual == null) return RedirectToAction("Login", "Home");

            
            var catalogoDb = await _tiendaService.ObtenerCatalogo();
            ObjetoTienda? objeto = catalogoDb.Find(item => item.Id == objetoId);

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
                
                HttpContext.Session.SetInt32("IdUsuario", resultadoLogin.IdUsuario);
                HttpContext.Session.SetString("NombreUsuario", resultadoLogin.Nombre);
                HttpContext.Session.SetInt32("PuntosUsuario", resultadoLogin.Kilometros);
                
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, resultadoLogin?.Message ?? "Correo o contraseña incorrectos.");
            
            return View("~/Views/Home/Login.cshtml", model);
        }

        
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        
        private async Task<TiendaViewModel> ConstruirTiendaViewModelAsync()
        {
            var objetos = new List<ObjetoTienda>();
            var objetosAdquiridos = ObtenerObjetosAdquiridos();
            var usuarioActual = ObtenerUsuarioActual();

            var catalogoDb = await _tiendaService.ObtenerCatalogo();

            foreach (var item in catalogoDb)
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
                else if (usuarioActual != null && item.CostoPuntos > usuarioActual.PuntosDisponibles)
                {
                    estado = EstadoObjetoTienda.Bloqueado;
                }

                item.Estado = estado;
                item.EsComprable = (estado == EstadoObjetoTienda.Disponible);
                item.TextoConfirmacion = ObtenerTextoConfirmacion(item.Nombre);

                objetos.Add(item);
            }

            var model = new TiendaViewModel
            {
                Usuario = usuarioActual ?? new UsuarioJuego(), 
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