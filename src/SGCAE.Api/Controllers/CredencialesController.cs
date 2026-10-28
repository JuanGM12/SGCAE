using Microsoft.AspNetCore.Mvc;
using SGCAE.Aplicacion.Abstracciones;
using SGCAE.Aplicacion.Credenciales;
using SGCAE.Dominio.Credenciales;

namespace SGCAE.Api.Controllers;

[ApiController]
[Route("api/credenciales")]
[Produces("application/json")]
public sealed class CredencialesController : ControllerBase
{
    /// <summary>RF-02: asignar credencial a empleado activo.</summary>
    [HttpPost]
    public async Task<ActionResult<CredencialDto>> Asignar([FromBody] AsignarCredencialComando comando,
        [FromServices] IComandoManejador<AsignarCredencialComando, CredencialDto> manejador, CancellationToken ct)
    {
        var credencial = await manejador.Ejecutar(comando, ct);
        return CreatedAtAction(nameof(Obtener), new { id = credencial.Id }, credencial);
    }

    /// <summary>RF-02: consultar estado de una credencial.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CredencialDto>> Obtener(Guid id,
        [FromServices] IConsultaManejador<ObtenerCredencialConsulta, CredencialDto> manejador, CancellationToken ct) =>
        Ok(await manejador.Ejecutar(new(id), ct));

    /// <summary>RF-02: listar credenciales de un empleado.</summary>
    [HttpGet("empleado/{empleadoId:guid}")]
    public async Task<ActionResult<IReadOnlyList<CredencialDto>>> PorEmpleado(Guid empleadoId,
        [FromServices] IConsultaManejador<ListarCredencialesEmpleadoConsulta, IReadOnlyList<CredencialDto>> manejador,
        CancellationToken ct) => Ok(await manejador.Ejecutar(new(empleadoId), ct));

    /// <summary>RF-02: bloquear una credencial no revocada.</summary>
    [HttpPatch("{id:guid}/bloquear")]
    public Task<ActionResult<CredencialDto>> Bloquear(Guid id,
        [FromServices] IComandoManejador<CambiarEstadoCredencialComando, CredencialDto> manejador,
        CancellationToken ct) => Cambiar(id, EstadoCredencial.Bloqueada, manejador, ct);

    /// <summary>RF-02: revocar definitivamente una credencial.</summary>
    [HttpPatch("{id:guid}/revocar")]
    public Task<ActionResult<CredencialDto>> Revocar(Guid id,
        [FromServices] IComandoManejador<CambiarEstadoCredencialComando, CredencialDto> manejador,
        CancellationToken ct) => Cambiar(id, EstadoCredencial.Revocada, manejador, ct);

    private static async Task<ActionResult<CredencialDto>> Cambiar(Guid id, EstadoCredencial estado,
        IComandoManejador<CambiarEstadoCredencialComando, CredencialDto> manejador, CancellationToken ct) =>
        new OkObjectResult(await manejador.Ejecutar(new(id, estado), ct));
}
