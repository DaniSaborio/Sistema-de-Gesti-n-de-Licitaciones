using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.Ofertas;

public sealed class OfertaEditViewModel
{
    public Guid Id { get; set; }
    public string LicitacionCodigo { get; set; } = string.Empty;
    public string ProveedorNombre { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
    [Display(Name = "Monto ofertado (CRC)")]
    public decimal MontoOfertadoCRC { get; set; }
}
