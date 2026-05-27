using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PinkPanther.Models;

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

        private static List<int> ObjetosAdquiridos = new List<int> { 3 };
        private static int? ObjetoEquipadoId = 6;

        private UsuarioJuego ObtenerUsuarioLogueado()
        {
            var nombre = HttpContext.Session.GetString("NombreUsuario");
            var puntos = HttpContext.Session.GetInt32("PuntosUsuario");

            if (nombre != null)
            {
                return new UsuarioJuego
                {
                    Nombre = nombre,
                    Rol = "Jugador",
                    PuntosDisponibles = puntos ?? 0
                };
            }
            
            return new UsuarioJuego { Nombre = "Invitado", Rol = "Ninguno", PuntosDisponibles = 0 };
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("NombreUsuario") == null)
            {
                return RedirectToAction("Login", "Home");
            }

            CargarDatosPanelUsuario();
            return View();
        }

        public IActionResult Tienda()
        {
            if (HttpContext.Session.GetString("NombreUsuario") == null)
            {
                return RedirectToAction("Login", "Home");
            }

            CargarDatosPanelUsuario();
            var usuario = ObtenerUsuarioLogueado();
            var model = ConstruirTiendaViewModel(usuario);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Comprar(int objetoId)
        {
            if (HttpContext.Session.GetString("NombreUsuario") == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var usuario = ObtenerUsuarioLogueado();
            ObjetoTienda? objeto = CatalogoBase.Find(item => item.Id == objetoId);

            if (objeto == null)
            {
                TempData["CompraError"] = "No se encontró el objeto seleccionado.";
                return RedirectToAction("Tienda");
            }

            if (ObjetosAdquiridos.Contains(objeto.Id) || ObjetoEquipadoId == objeto.Id)
            {
                TempData["CompraError"] = objeto.Nombre + " ya forma parte de tu inventario.";
                return RedirectToAction("Tienda");
            }

            if (objeto.CostoPuntos > usuario.PuntosDisponibles)
            {
                TempData["CompraError"] = "No tienes puntos suficientes para comprar " + objeto.Nombre + ".";
                return RedirectToAction("Tienda");
            }

            ObjetosAdquiridos.Add(objeto.Id);
            TempData["CompraExitosa"] = "Compra realizada: " + objeto.Nombre + " por " + objeto.CostoPuntos.ToString("N0") + " puntos.";
            return RedirectToAction(nameof(Tienda));
        }

        private void CargarDatosPanelUsuario()
        {
            var usuario = ObtenerUsuarioLogueado();
            ViewData["NombreUsuario"] = usuario.Nombre;
            ViewData["RolUsuario"] = usuario.Rol;
            ViewData["PuntosUsuario"] = usuario.PuntosDisponibles.ToString("N0");
        }

        private static TiendaViewModel ConstruirTiendaViewModel(UsuarioJuego usuarioActual)
        {
            var objetos = new List<ObjetoTienda>();

            foreach (var item in CatalogoBase)
            {
                var estado = EstadoObjetoTienda.Disponible;

                if (ObjetoEquipadoId == item.Id)
                {
                    estado = EstadoObjetoTienda.Equipado;
                }
                else if (ObjetosAdquiridos.Contains(item.Id))
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
    }
}