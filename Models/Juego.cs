namespace PinkPanther.Models;

public class Juego
{
    public int IdJuego { get; set; }

    public string NombreJuego { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Objetivos { get; set; } = string.Empty;

    public string LinkImagen { get; set; } = string.Empty;

    public string Dificultad { get; set; } = string.Empty;

    public string Duracion { get; set; } = string.Empty;

    public string BuildFolder { get; set; } = string.Empty;
    
    public int IdUsuario{ get; set; }
    }
