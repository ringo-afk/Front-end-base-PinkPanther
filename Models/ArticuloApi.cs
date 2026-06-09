namespace PinkPanther.Models
{
    public class ArticuloApi
    {
        public int id { get; set; }
        public string tipo { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public int precio { get; set; }
        public string rareza { get; set; }
        public bool equipado { get; set; }
    }
}