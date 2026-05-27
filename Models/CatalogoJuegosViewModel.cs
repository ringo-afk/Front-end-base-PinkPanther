namespace PinkPanther.Models;

public class CatalogoJuegosViewModel
{
    public List<Juego> Juegos { get; set; } = new();

    public string? DificultadSeleccionada { get; set; }
}
