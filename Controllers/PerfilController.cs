using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using PinkPanther.Models;
using System.Collections.Generic;

namespace PinkPanther.Controllers;

[Route("Perfil")]
public class PerfilController : Controller
{
    private static string resumenGuardado = "Profesional con experiencia en la gestion de proyectos y coordinacion de equipos.";

    private readonly ILogger<PerfilController> _logger;

    public PerfilController(ILogger<PerfilController> logger)
    {
        _logger = logger;
    }

    [HttpGet("")]
    public IActionResult Perfil()
    {
        if (HttpContext.Session.GetString("NombreUsuario") == null)
        {
            return RedirectToAction("Login", "Home");
        }

        PerfilViewModel perfil = new PerfilViewModel();

        perfil.Nombre = "Ana García";
        perfil.Puesto = "Gerente Senior de Proyectos";
        perfil.Nivel = "Level 18 - Experto";
        perfil.PuntosDigitales = 3500;

        perfil.Departamento = "Operaciones";
        perfil.Ubicacion = "Monterrey";

        perfil.ResumenProfesional = resumenGuardado;

        perfil.Logros = new List<Logro>()
        {
            new Logro
            {
                Icono = "star",
                Titulo = "Mejor Desempeño",
                Descripcion = "Supera tus objetivos de ventas por 3 meses consecutivos"
            },
            new Logro
            {
                Icono = "shield",
                Titulo = "Solucionador de Problemas",
                Descripcion = "Resuelve 100 casos de clientes con 95% de satisfaccion"
            }
        };

        perfil.ArticulosVirtuales = new List<ArticuloVirtual>()
        {
            new ArticuloVirtual
            {
                Icono = "gift",
                Nombre = "Paquete de Puntos Bonus",
                Rareza = "Ultra Raro",
                EsUltraRaro = true
            },
            new ArticuloVirtual
            {
                Icono = "shirt",
                Nombre = "Camiseta Virtual",
                Rareza = "Raro",
                EsUltraRaro = false
            }
        };

        perfil.Habilidades = new List<Habilidad>()
        {
            new Habilidad { Nombre = "Liderazgo" },
            new Habilidad { Nombre = "Gestión" },
            new Habilidad { Nombre = "Resolución" }
        };

        ViewBag.NombreUsuario = perfil.Nombre;

        return View("~/Views/Home/Perfil.cshtml", perfil);
    }

    [HttpPost("GuardarResumen")]
    public IActionResult GuardarResumen(string ResumenProfesional)
    {
        if (HttpContext.Session.GetString("NombreUsuario") == null)
        {
            return RedirectToAction("Login", "Home");
        }

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
            resumenGuardado = ResumenProfesional;
            TempData["Mensaje"] = "Resumen profesional guardado correctamente.";
        }

        TempData["AbrirModal"] = "true";

        return RedirectToAction("Perfil");
    }
}