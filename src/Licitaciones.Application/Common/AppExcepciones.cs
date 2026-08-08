namespace Licitaciones.Application.Common;

/// <summary>Recurso no encontrado; la capa Api/Web la traduce a HTTP 404.</summary>
public sealed class RecursoNoEncontradoException(string recurso, Guid id)
    : Exception($"{recurso} '{id}' no fue encontrado.");

/// <summary>Violación de unicidad detectada en la capa de aplicación; se traduce a HTTP 409.</summary>
public sealed class ConflictoDeUnicidadException(string mensaje) : Exception(mensaje);
