namespace PinkPanther.Models;

public class TiendaViewModel
{
    public UsuarioJuego Usuario { get; set; } = new();

    public List<ObjetoTienda> Objetos { get; set; } = new();
}
