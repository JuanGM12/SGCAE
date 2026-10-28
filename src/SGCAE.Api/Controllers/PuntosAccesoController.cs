using Microsoft.AspNetCore.Mvc;
using SGCAE.Aplicacion.Abstracciones;
using SGCAE.Aplicacion.PuntosAcceso;

namespace SGCAE.Api.Controllers;

[ApiController]
[Route("api/puntos-acceso")]
[Produces("application/json")]
public sealed class PuntosAccesoController : ControllerBase
{
    /// <summary>RF-06: registrar un punto de acceso físico.</summary>
    [HttpPost]
    public async Task<ActionResult<PuntoAccesoDto>> Crear([FromBody] RegistrarPuntoAccesoComando comando,
        [FromServices] IComandoManejador<RegistrarPuntoAccesoComando, PuntoAccesoDto> manejador, CancellationToken ct)
    {
        var punto = await manejador.Ejecutar(comando, ct);
        return CreatedAtAction(nameof(Obtener), new { id = punto.Id }, punto);
    }

    /// <summary>RF-06: consultar punto de acceso.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PuntoAccesoDto>> Obtener(Guid id,
        [FromServices] IConsultaManejador<ObtenerPuntoAccesoConsulta, PuntoAccesoDto> manejador, CancellationToken ct) =>
        Ok(await manejador.Ejecutar(new(id), ct));

    /// <summary>RF-06: consultar todos los puntos de acceso.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PuntoAccesoDto>>> Listar(
        [FromServices] IConsultaManejador<ListarPuntosAccesoConsulta, IReadOnlyList<PuntoAccesoDto>> manejador,
        CancellationToken ct) => Ok(await manejador.Ejecutar(new(), ct));

    /// <summary>RF-06: habilitar o deshabilitar un punto.</summary>
    [HttpPatch("{id:guid}/estado")]
    public async Task<ActionResult<PuntoAccesoDto>> CambiarEstado(Guid id, [FromBody] EstadoPuntoPeticion peticion,
        [FromServices] IComandoManejador<CambiarEstadoPuntoAccesoComando, PuntoAccesoDto> manejador,
        CancellationToken ct) => Ok(await manejador.Ejecutar(new(id, peticion.Habilitado), ct));
}

public sealed record EstadoPuntoPeticion(bool Habilitado);
