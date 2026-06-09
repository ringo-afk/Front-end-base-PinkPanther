namespace PinkPanther.Models
{
    public class PerfilApiResponse
    {
        public int idUsuario { get; set; }
        public string nombre { get; set; }
        public string puesto { get; set; }
        public string nivel { get; set; }
        public string puntosDigitales { get; set; }
        public string departamento { get; set; }
        public string ubicacion { get; set; }
        public string? fotoDePerfil { get; set; }
        public string resumenProfesional { get; set; }

        public List<LogroApi> logros { get; set; }
        public List<ArticuloApi> articulosVirtuales { get; set; }
    }
}