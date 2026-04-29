using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PinkPanther.Models;

namespace PinkPanther.Controllers
{
    public class HomeController : Controller
    {

        // aquí guardamos el usuario de prueba que se muestra en la pantalla
        private static readonly UsuarioJuego UsuarioActual = new UsuarioJuego
        {
            Nombre = "Ana García",
            Rol = "Jugador",
            PuntosDisponibles = 5000
        };

        // este es el catálogo falso que usamos para simular la tienda
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

        // aquí marcamos los objetos que ya se compraron en la demo
        private static List<int> ObjetosAdquiridos = new List<int> { 3 };
        // este objeto arranca equipado para que la vista ya tenga un caso especial
        private static int? ObjetoEquipadoId = 6;

        public IActionResult Index()
        {
            // llenamos los datos básicos del panel antes de mostrar la vista
            CargarDatosPanelUsuario();
            return View();
        }

        public IActionResult Privacy()
        {
            // la vista de perfil también necesita los datos del usuario
            CargarDatosPanelUsuario();
            return View();
        }

        public IActionResult Tienda()
        {
            // aquí armamos todo lo que necesita la pantalla de tienda
            CargarDatosPanelUsuario();
            var model = ConstruirTiendaViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Comprar(int objetoId)
        {
            
            // buscamos el producto que el usuario intentó comprar
            ObjetoTienda? objeto = CatalogoBase.Find(item => item.Id == objetoId);

            if (objeto == null)
            {
                // si no existe, regresamos el error a la vista
                TempData["CompraError"] = "No se encontró el objeto seleccionado.";
                return RedirectToAction("Tienda");
            }

            if (ObjetosAdquiridos.Contains(objeto.Id) || ObjetoEquipadoId == objeto.Id)
            {
                // si ya lo tiene o está equipado, no dejamos repetir la compra
                TempData["CompraError"] = objeto.Nombre + " ya forma parte de tu inventario.";
                return RedirectToAction("Tienda");
            }

            if (objeto.CostoPuntos > UsuarioActual.PuntosDisponibles)
            {
                // aquí revisamos que sí haya puntos suficientes
                TempData["CompraError"] = "No tienes puntos suficientes para comprar " + objeto.Nombre + ".";
                return RedirectToAction("Tienda");
            }

            // si pasa todos los filtros, descontamos puntos y guardamos la compra
            UsuarioActual.PuntosDisponibles -= objeto.CostoPuntos;
            ObjetosAdquiridos.Add(objeto.Id);
            TempData["CompraExitosa"] = "Compra realizada: " + objeto.Nombre + " por " + objeto.CostoPuntos.ToString("N0") + " puntos.";
            return RedirectToAction(nameof(Tienda));
        }

        private void CargarDatosPanelUsuario()
        {
            // mandamos la info del usuario a la cabecera y al layout
            ViewData["NombreUsuario"] = UsuarioActual.Nombre;
            ViewData["RolUsuario"] = UsuarioActual.Rol;
            ViewData["PuntosUsuario"] = UsuarioActual.PuntosDisponibles.ToString("N0");
        }

        private static TiendaViewModel ConstruirTiendaViewModel()
        {
            var objetos = new List<ObjetoTienda>();

            // recorremos todo el catálogo para decidir qué estado tiene cada artículo
            foreach (var item in CatalogoBase)
            {
                var estado = EstadoObjetoTienda.Disponible;

                if (ObjetoEquipadoId == item.Id)
                {
                    // si es el equipado, lo mostramos como equipado
                    estado = EstadoObjetoTienda.Equipado;
                }
                else if (ObjetosAdquiridos.Contains(item.Id))
                {
                    // si ya fue comprado, lo marcamos como adquirido
                    estado = EstadoObjetoTienda.Adquirido;
                }
                else if (item.CostoPuntos > UsuarioActual.PuntosDisponibles)
                {
                    // si no alcanza el saldo, queda bloqueado
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

            // clonamos los datos del usuario para mandarlos al modelo de la vista
            var usuario = new UsuarioJuego
            {
                Nombre = UsuarioActual.Nombre,
                Rol = UsuarioActual.Rol,
                PuntosDisponibles = UsuarioActual.PuntosDisponibles
            };

            var model = new TiendaViewModel
            {
                Usuario = usuario,
                Objetos = objetos
            };

            return model;
        }

        private static string ObtenerTextoConfirmacion(string nombreObjeto)
        {
            // esto es para que el texto suene más natural al confirmar la compra
            if (nombreObjeto == "Chamarra Élite")
             return "la Chamarra Élite";
            if (nombreObjeto == "Lentes VR de Neón") 
            return "los Lentes VR de Neón";
            if (nombreObjeto == "Rines Carro") 
            return "los Rines Carro";
            if (nombreObjeto == "Carro Moderno") 
            return "el Carro Moderno";
            if (nombreObjeto == "Gorra Negra") 
            return "la Gorra Negra";
            if (nombreObjeto == "Casco Ingeniero")
             return "el Casco Ingeniero";
            if (nombreObjeto == "Objeto Aleatorio") 
            return "el Objeto Aleatorio";
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
