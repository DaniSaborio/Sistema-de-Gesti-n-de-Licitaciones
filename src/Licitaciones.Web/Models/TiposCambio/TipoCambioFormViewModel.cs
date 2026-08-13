using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.TiposCambio;

public sealed class TipoCambioFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [Range(0.000001, double.MaxValue, ErrorMessage = "El tipo de cambio debe ser mayor que cero.")]
    [Display(Name = "Colones por dólar (CRC/USD)")]
    public decimal CRCporUSD { get; set; }

    [Required(ErrorMessage = "La fecha de vigencia es obligatoria.")]
    [Display(Name = "Fecha de vigencia")]
    [DataType(DataType.Date)]
    public DateTime FechaVigencia { get; set; } = DateTime.Today;
}
