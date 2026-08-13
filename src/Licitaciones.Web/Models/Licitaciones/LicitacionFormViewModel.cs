using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.Licitaciones;

public sealed class LicitacionFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha y hora de cierre son obligatorias.")]
    [Display(Name = "Fecha y hora de cierre")]
    [DataType(DataType.DateTime)]
    public DateTime FechaCierre { get; set; } = DateTime.Now.AddDays(7);

    [Required(ErrorMessage = "El presupuesto estimado es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El presupuesto debe ser mayor que cero.")]
    [Display(Name = "Presupuesto estimado (CRC)")]
    public decimal PresupuestoEstimadoCRC { get; set; }

    public bool EsEdicion => Id.HasValue;
}
