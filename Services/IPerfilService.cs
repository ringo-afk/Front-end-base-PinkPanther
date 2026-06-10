using PinkPanther.Models;

namespace PinkPanther.Services
{
    public interface IPerfilService
    {
        Task<PerfilViewModel?> ObtenerPerfil(int idUsuario);
        Task<bool> GuardarResumen(int idUsuario, string resumenProfesional);
        Task<bool?> EquiparArticulo(int idUsuario, int idArticulo, string tipoArticulo);
    }
}