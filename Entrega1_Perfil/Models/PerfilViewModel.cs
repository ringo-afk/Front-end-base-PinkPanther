using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entrega1_Perfil.Models
{
    public class PerfilViewModel
    {
        public string Nombre { get; set; }
        public string Puesto { get; set; }
        public string Nivel { get; set; }
        public int PuntosDigitales { get; set; }

        public string Departamento { get; set; }
        public string Ubicacion { get; set; }
        [Required(ErrorMessage = "El resumen profesional es requerido")]
        [StringLength(400, ErrorMessage = "El resumen no puede pasar de 400 caracteres")]
        public string ResumenProfesional { get; set; }

        public List<Logro> Logros { get; set; }
        public List<ArticuloVirtual> ArticulosVirtuales { get; set; }
        public List<Habilidad> Habilidades { get; set; }
    }
}