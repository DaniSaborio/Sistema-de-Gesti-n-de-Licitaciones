namespace Licitaciones.Web.Models.Ofertas;

public sealed class OfertaDetalleViewModel
{
    public Guid Id { get; set; }
    public Guid LicitacionId { get; set; }
    public string LicitacionCodigo { get; set; } = string.Empty;
    public string LicitacionTitulo { get; set; } = string.Empty;
    public string ProveedorNombre { get; set; } = string.Empty;
    public decimal MontoOfertadoCRC { get; set; }
    public DateTimeOffset FechaRegistro { get; set; }
}