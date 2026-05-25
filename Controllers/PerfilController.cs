using Microsoft.AspNetCore.Mvc;
using Entrega1_Perfil.Models;
using System.Collections.Generic;

namespace PinkPanther.Controllers;

public class PerfilController : Controller
{
    // Guarda temporalmente el resumen mientras corre la app
    private static string resumenGuardado = "Profesional con experiencia en la gestion de proyectos y coordinacion de equipos.";

    private readonly ILogger<PerfilController> _logger;

    public PerfilController(ILogger<PerfilController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Crear objeto perfil con datos simulados
        PerfilViewModel perfil = new PerfilViewModel();

        perfil.Nombre = "Ana García";
        perfil.Puesto = "Gerente Senior de Proyectos";
        perfil.Nivel = "Level 18 - Experto";
        perfil.PuntosDigitales = 3500;

        perfil.Departamento = "Operaciones";
        perfil.Ubicacion = "Monterrey";

        perfil.ResumenProfesional = resumenGuardado;

        // Lista de logros
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

        // Lista de articulos virtuales
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

        // Lista de habilidades
        perfil.Habilidades = new List<Habilidad>()
        {
            new Habilidad { Nombre = "Liderazgo" },
            new Habilidad { Nombre = "Gestión" },
            new Habilidad { Nombre = "Resolución" }
        };

        // Mandar nombre al layout
        ViewBag.NombreUsuario = perfil.Nombre;

        // Usar la vista que dejaste en Views/Home/Perfil.cshtml
        return View("~/Views/Home/Perfil.cshtml", perfil);
    }

    [HttpPost]
    public IActionResult GuardarResumen(string ResumenProfesional)
    {
        // Validar resumen profesional
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

        return RedirectToAction("Index");
    }
}