using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using PinkPanther.Models;
using PinkPanther.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace PinkPanther.Controllers;


[Route("Perfil")]
public class PerfilController : Controller
{
    private static string resumenGuardado = "Profesional con experiencia en la gestion de proyectos y coordinacion de equipos.";
    private static string rutaFotoGuardada = null;

    private readonly ILogger<PerfilController> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly IPerfilService _perfilService;

    public PerfilController(
        ILogger<PerfilController> logger,
        IWebHostEnvironment env,
        IPerfilService perfilService)
    {
        _logger = logger;
        _env = env;
        _perfilService = perfilService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Perfil()
    {
        if (HttpContext.Session.GetString("NombreUsuario") == null)
        {
            return RedirectToAction("Login", "Home");
        }

        int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 1;

        PerfilViewModel perfil = await _perfilService.ObtenerPerfil(idUsuario);

        if (perfil == null)
        {
            perfil = new PerfilViewModel();

            perfil.Nombre = "Usuario no encontrado";
            perfil.Puesto = "Sin puesto";
            perfil.Nivel = "Sin nivel";
            perfil.PuntosDigitales = 0;
            perfil.Departamento = "Sin departamento";
            perfil.Ubicacion = "Sin ubicacion";
            perfil.ResumenProfesional = resumenGuardado;
            perfil.RutaFotoPerfil = rutaFotoGuardada;

            perfil.Logros = new List<Logro>();
            perfil.ArticulosVirtuales = new List<ArticuloVirtual>();
            perfil.Habilidades = new List<Habilidad>();
        }
        else
        {
            perfil.RutaFotoPerfil = rutaFotoGuardada;
        }

        ViewBag.NombreUsuario = perfil.Nombre;

        return View("~/Views/Home/Perfil.cshtml", perfil);
    }

    [HttpPost("GuardarResumen")]
    public async Task<IActionResult> GuardarResumen(string ResumenProfesional)
    {
        if (HttpContext.Session.GetString("NombreUsuario") == null)
        {
            return RedirectToAction("Login", "Home");
        }

        int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 1;

        if (string.IsNullOrWhiteSpace(ResumenProfesional))
        {
            TempData["Error"] = "El resumen profesional es requerido.";
        }
        else if (ResumenProfesional.Length > 400)
        {
            TempData["Error"] = "El resumen no puede pasar de 400 caracteres.";
        }
        else
        {
            bool guardado = await _perfilService.GuardarResumen(idUsuario, ResumenProfesional);

            if (guardado)
            {
                TempData["Mensaje"] = "Resumen profesional guardado correctamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo guardar el resumen profesional.";
            }
        }

        TempData["AbrirModal"] = "true";

        return RedirectToAction("Perfil");
    }

    [HttpPost("GuardarFoto")]
    public async Task<IActionResult> GuardarFoto(PerfilViewModel perfil)
    {
        if (HttpContext.Session.GetString("NombreUsuario") == null)
        {
            return RedirectToAction("Login", "Home");
        }

        if (perfil.FotoPerfil != null && perfil.FotoPerfil.Length > 0)
        {
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(perfil.FotoPerfil.FileName).ToLower();

            if (!extensionesPermitidas.Contains(extension))
            {
                TempData["ErrorFoto"] = "Solo se permiten imagenes jpg, jpeg, png o gif.";
            }
            else
            {
                string carpeta = Path.Combine(_env.WebRootPath, "Imagenes/fotosperfil");

                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }

                string nombreArchivo = Guid.NewGuid().ToString() + "_FotoPerfil" + extension;
                string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await perfil.FotoPerfil.CopyToAsync(stream);
                }

                rutaFotoGuardada = "/Imagenes/fotosperfil/" + nombreArchivo;

                TempData["MensajeFoto"] = "Foto de perfil guardada correctamente.";
            }
        }
        else
        {
            TempData["ErrorFoto"] = "Selecciona una foto primero.";
        }

        TempData["AbrirModal"] = "true";

        return RedirectToAction("Perfil");
    }
}