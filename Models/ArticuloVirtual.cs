namespace PinkPanther.Models
{
    public class ArticuloVirtual
    {
        public int Id { get; set; }
        public string Tipo { get; set; }

        public string Icono { get; set; }
        public string Nombre { get; set; }
        public string Rareza { get; set; }
        public bool EsUltraRaro { get; set; }

        public bool Equipado { get; set; }
    }
}