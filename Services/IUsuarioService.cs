namespace PinkPanther.Services;
using PinkPanther.Models;
public interface IUsuarioService
{
    Task<List<Usuario>> ObtenerUsuarios();
}