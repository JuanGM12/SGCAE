using Microsoft.EntityFrameworkCore;
using SGCAE.Dominio.Compartido;

namespace SGCAE.Api.Middleware;

public sealed class ControlErroresMiddleware(RequestDelegate siguiente, ILogger<ControlErroresMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext contexto)
    {
        try { await siguiente(contexto); }
        catch (Exception ex)
        {
            if (contexto.Response.HasStarted) throw;
            int estado = ex switch
            {
                KeyNotFoundException => StatusCodes.Status404NotFound,
                ReglaNegocioException => StatusCodes.Status409Conflict,
                ArgumentException => StatusCodes.Status400BadRequest,
                DbUpdateException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
            if (estado >= 500) logger.LogError(ex, "Error inesperado en SGCAE");
            else logger.LogWarning(ex, "Solicitud rechazada por validación/regla de negocio");
            contexto.Response.StatusCode = estado;
            await contexto.Response.WriteAsJsonAsync(new
            {
                tipo = "error", estado, mensaje = estado == 500
                    ? "Error interno. Consulte los registros del servidor." : ex.Message
            });
        }
    }
}
