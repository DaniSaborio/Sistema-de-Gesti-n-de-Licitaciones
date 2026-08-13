namespace Licitaciones.Web.Models.Shared;

public sealed class PaginacionViewModel
{
    public required string Accion { get; init; }
    public object? RouteValues { get; init; }
    public int PaginaActual { get; init; }
    public int TotalPaginas { get; init; }
}
