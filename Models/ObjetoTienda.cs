namespace PinkPanther.Models;

public class ObjetoTienda
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public int CostoPuntos { get; set; }

    public string RutaImagen { get; set; } = string.Empty;

    public string TextoConfirmacion { get; set; } = string.Empty;

    public bool EsComprable { get; set; }

    public EstadoObjetoTienda Estado { get; set; }
}
