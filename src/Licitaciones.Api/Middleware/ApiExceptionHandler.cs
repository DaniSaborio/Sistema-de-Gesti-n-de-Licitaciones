using FluentValidation;
using Licitaciones.Application.Common;
using Licitaciones.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Api.Middleware;

/// <summary>
/// Traduce excepciones de dominio/aplicación a respuestas ProblemDetails
/// controladas para las rutas /api (sección 10.2, 11): nunca expone stack
/// traces, rutas internas ni mensajes técnicos crudos al cliente.
/// </summary>
public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (!httpContext.Request.Path.StartsWithSegments("/api"))
        {
            return false;
        }

        var (status, title, detail) = Clasificar(exception);

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Error no controlado al procesar {Metodo} {Ruta}", httpContext.Request.Method, httpContext.Request.Path);
        }

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.io/{status}",
            Instance = httpContext.Request.Path,
        };
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["errorCode"] = exception.GetType().Name;

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errores"] = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        httpContext.Response.StatusCode = status;
        // WriteAsJsonAsync ignora un Response.ContentType asignado de antemano y lo
        // sobrescribe con "application/json" salvo que se le pase explícitamente;
        // el contentType debe ir como argumento de este overload, no como asignación previa.
        await httpContext.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }

    private static (int Status, string Title, string? Detail) Clasificar(Exception exception) => exception switch
    {
        RecursoNoEncontradoException => (StatusCodes.Status404NotFound, "Recurso no encontrado", exception.Message),
        ConflictoDeUnicidadException => (StatusCodes.Status409Conflict, "Conflicto de unicidad", exception.Message),
        ConflictoDeConcurrenciaException => (StatusCodes.Status409Conflict, "Conflicto de concurrencia",
            "El recurso fue modificado por otra operación; recárguelo e intente de nuevo."),
        ValidationException => (StatusCodes.Status400BadRequest, "Solicitud inválida", "Uno o más campos no son válidos."),
        DomainException => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio no satisfecha", exception.Message),
        ErrorDeIntegridadDeDatosException => (StatusCodes.Status400BadRequest, "No se pudo completar la operación",
            "La operación viola una restricción de integridad de datos."),
        _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor",
            "Ocurrió un error inesperado. Contacte al administrador con el identificador de correlación."),
    };
}
