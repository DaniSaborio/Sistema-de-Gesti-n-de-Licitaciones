using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.NivelesAprobacion;

public sealed class NivelAprobacionFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto mínimo debe ser mayor que cero.")]
    [Display(Name = "Monto mínimo (CRC)")]
    public decimal MontoMinimoCRC { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El monto máximo debe ser mayor que cero.")]
    [Display(Name = "Monto máximo (CRC) — vacío para rango abierto")]
    public decimal? MontoMaximoCRC { get; set; }

    [Required(ErrorMessage = "El aprobador es obligatorio.")]
    [StringLength(150)]
    [Display(Name = "Aprobador")]
    public string Aprobador { get; set; } = string.Empty;
}
