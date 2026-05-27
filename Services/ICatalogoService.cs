using PinkPanther.Models;

namespace PinkPanther.Services;
public interface ICatalogoService
{
    Task<List<Juego>> ObtenerJuegosAsync();
    Task<List<Juego>> ObtenerJuegosPorDificultadAsync(string dificultad);
}
