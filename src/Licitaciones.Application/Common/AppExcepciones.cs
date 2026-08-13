namespace Licitaciones.Application.Common;

/// <summary>Recurso no encontrado; la capa Api/Web la traduce a HTTP 404.</summary>
public sealed class RecursoNoEncontradoException(string recurso, Guid id)
    : Exception($"{recurso} '{id}' no fue encontrado.");

/// <summary>Violación de unicidad detectada en la capa de aplicación; se traduce a HTTP 409.</summary>
public sealed class ConflictoDeUnicidadException(string mensaje) : Exception(mensaje);

/// <summary>
/// Envuelve un DbUpdateConcurrencyException capturado en Infrastructure, para que
/// Application/Api no dependan de Entity Framework Core; se traduce a HTTP 409.
/// </summary>
public sealed class ConflictoDeConcurrenciaException(string mensaje, Exception innerException)
    : Exception(mensaje, innerException);

/// <summary>
/// Envuelve un DbUpdateException (violación de restricción/FK) capturado en
/// Infrastructure; se traduce a HTTP 400 sin exponer detalles técnicos.
/// </summary>
public sealed class ErrorDeIntegridadDeDatosException(string mensaje, Exception innerException)
    : Exception(mensaje, innerException);
