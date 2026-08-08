using Licitaciones.Domain.Common;

namespace Licitaciones.Domain.Proveedores;

public sealed class Proveedor : SoftDeletableEntity
{
    public string Nombre { get; private set; } = default!;
    public string NombreNormalizado { get; private set; } = default!;

    private Proveedor() { }

    public static Proveedor Crear(string nombre, IClock clock)
    {
        ValidarNombre(nombre);
        var ahora = clock.UtcNow;
        return new Proveedor
        {
            Nombre = nombre.Trim(),
            NombreNormalizado = NormalizacionTexto.Normalizar(nombre),
            CreatedAt = ahora,
            UpdatedAt = ahora,
        };
    }

    public void ActualizarNombre(string nombre, IClock clock)
    {
        ValidarNombre(nombre);
        Nombre = nombre.Trim();
        NombreNormalizado = NormalizacionTexto.Normalizar(nombre);
        Touch(clock);
    }

    private static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new NombreProveedorInvalidoException("El nombre del proveedor es obligatorio.");
        }

        if (!NormalizacionTexto.TieneCaracteresPermitidosParaProveedor(nombre))
        {
            throw new NombreProveedorInvalidoException(
                "El nombre del proveedor solo admite letras, números, espacios, punto, coma y paréntesis.");
        }
    }
}

public sealed class NombreProveedorInvalidoException : DomainException
{
    public NombreProveedorInvalidoException(string mensaje) : base(mensaje) { }
}

public sealed class ProveedorConOfertasException : DomainException
{
    public ProveedorConOfertasException()
        : base("No se puede eliminar físicamente un proveedor con ofertas relacionadas.") { }
}
