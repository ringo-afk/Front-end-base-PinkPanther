using PinkPanther.Models;

namespace PinkPanther.Services
{
    public interface ITiendaService
    {
        Task<List<ObjetoTienda>> ObtenerCatalogo();
    }
}