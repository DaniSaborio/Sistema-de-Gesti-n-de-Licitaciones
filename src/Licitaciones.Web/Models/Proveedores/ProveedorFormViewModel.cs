using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.Proveedores;

public sealed class ProveedorFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;
}
