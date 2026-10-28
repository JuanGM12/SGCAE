using Microsoft.AspNetCore.Mvc;
using SGCAE.Aplicacion.Abstracciones;
using SGCAE.Aplicacion.Accesos;
using SGCAE.Dominio.Accesos;

namespace SGCAE.Api.Controllers;

[ApiController]
[Route("api/accesos")]
[Produces("application/json")]
public sealed class AccesosController : ControllerBase
{
    /// <summary>RF-03: registra intentos de entrada/salida. Un rechazo devuelve 200 con resultado Rechazado, pues queda auditado.</summary>
    [HttpPost]
    public async Task<ActionResult<MovimientoAccesoDto>> Registrar([FromBody] RegistrarAccesoComando comando,
        [FromServices] IComandoManejador<RegistrarAccesoComando, MovimientoAccesoDto> manejador,
        CancellationToken ct) => Ok(await manejador.Ejecutar(comando, ct));

    /// <summary>RF-04: historial filtrable por empleado, fechas UTC, código de punto, resultado y paginación.</summary>
    [HttpGet("historial")]
    public async Task<ActionResult<Pagina<MovimientoAccesoDto>>> Historial(
        [FromQuery] FiltroHistorialPeticion filtro,
        [FromServices] IConsultaManejador<ConsultarHistorialConsulta, Pagina<MovimientoAccesoDto>> manejador,
        CancellationToken ct) => Ok(await manejador.Ejecutar(new ConsultarHistorialConsulta(
            filtro.EmpleadoId, filtro.DesdeUtc, filtro.HastaUtc, filtro.CodigoPuntoAcceso,
            filtro.Resultado, filtro.Pagina, filtro.TamanoPagina), ct));
}

public sealed class FiltroHistorialPeticion
{
    public Guid? EmpleadoId { get; set; }
    public DateTime? DesdeUtc { get; set; }
    public DateTime? HastaUtc { get; set; }
    public string? CodigoPuntoAcceso { get; set; }
    public ResultadoAcceso? Resultado { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 20;
}
