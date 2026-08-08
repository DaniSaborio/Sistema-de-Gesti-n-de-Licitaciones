namespace Licitaciones.Application.Common;

public sealed record ConsultaPaginada(
    int Pagina = 1,
    int TamanoPagina = 10,
    string? Busqueda = null,
    string? OrdenarPor = null,
    bool Descendente = false)
{
    public int Pagina { get; init; } = Pagina < 1 ? 1 : Pagina;
    public int TamanoPagina { get; init; } = TamanoPagina is < 1 or > 100 ? 10 : TamanoPagina;
}

public sealed record ResultadoPaginado<T>(IReadOnlyList<T> Elementos, int TotalElementos, int Pagina, int TamanoPagina)
{
    public int TotalPaginas => TotalElementos == 0 ? 0 : (int)Math.Ceiling(TotalElementos / (double)TamanoPagina);
}
