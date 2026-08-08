using System.Text;
using System.Text.RegularExpressions;

namespace Licitaciones.Domain.Common;

/// <summary>
/// Normalización reutilizada para garantizar unicidad de código de licitación
/// y nombre de proveedor: recorta espacios laterales, colapsa espacios internos,
/// normaliza Unicode (NFKC) y compara sin distinguir mayúsculas/minúsculas.
/// </summary>
public static partial class NormalizacionTexto
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex EspaciosMultiples();

    [GeneratedRegex(@"^[\p{L}\p{N}\.,\(\)\s]+$")]
    private static partial Regex CaracteresPermitidosProveedor();

    public static string Normalizar(string valor)
    {
        ArgumentNullException.ThrowIfNull(valor);

        var recortado = valor.Trim();
        var colapsado = EspaciosMultiples().Replace(recortado, " ");
        var formaNormalizada = colapsado.Normalize(NormalizationForm.FormKC);
        return formaNormalizada.ToUpperInvariant();
    }

    public static bool TieneCaracteresPermitidosParaProveedor(string valor) =>
        CaracteresPermitidosProveedor().IsMatch(valor);
}
