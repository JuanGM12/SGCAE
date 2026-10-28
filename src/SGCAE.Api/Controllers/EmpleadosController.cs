using Microsoft.AspNetCore.Mvc;
using SGCAE.Aplicacion.Abstracciones;
using SGCAE.Aplicacion.Empleados;

namespace SGCAE.Api.Controllers;

[ApiController]
[Route("api/empleados")]
[Produces("application/json")]
public sealed class EmpleadosController : ControllerBase
{
    /// <summary>RF-01: registrar un empleado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<EmpleadoDto>> Crear([FromBody] CrearEmpleadoComando comando,
        [FromServices] IComandoManejador<CrearEmpleadoComando, EmpleadoDto> manejador, CancellationToken ct)
    {
        var empleado = await manejador.Ejecutar(comando, ct);
        return CreatedAtAction(nameof(Obtener), new { id = empleado.Id }, empleado);
    }

    /// <summary>RF-01: obtener un empleado por ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmpleadoDto>> Obtener(Guid id,
        [FromServices] IConsultaManejador<ObtenerEmpleadoConsulta, EmpleadoDto> manejador, CancellationToken ct) =>
        Ok(await manejador.Ejecutar(new ObtenerEmpleadoConsulta(id), ct));

    /// <summary>RF-01: listar empleados paginados.</summary>
    [HttpGet]
    public async Task<ActionResult<Pagina<EmpleadoDto>>> Listar([FromQuery] FiltroEmpleadosPeticion filtro,
        [FromServices] IConsultaManejador<ListarEmpleadosConsulta, Pagina<EmpleadoDto>> manejador,
        CancellationToken ct) => Ok(await manejador.Ejecutar(new(filtro.Pagina, filtro.TamanoPagina), ct));

    /// <summary>RF-01: actualizar datos de un empleado.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmpleadoDto>> Actualizar(Guid id, [FromBody] DatosEmpleadoPeticion datos,
        [FromServices] IComandoManejador<ActualizarEmpleadoComando, EmpleadoDto> manejador, CancellationToken ct) =>
        Ok(await manejador.Ejecutar(new(id, datos.Documento, datos.Nombres, datos.Apellidos,
            datos.Correo, datos.Cargo, datos.AreaOrganizacional), ct));

    /// <summary>RF-01: desactivar empleado; conserva su información e historial.</summary>
    [HttpPatch("{id:guid}/desactivar")]
    public async Task<ActionResult<EmpleadoDto>> Desactivar(Guid id,
        [FromServices] IComandoManejador<DesactivarEmpleadoComando, EmpleadoDto> manejador, CancellationToken ct) =>
        Ok(await manejador.Ejecutar(new(id), ct));
}

public sealed record DatosEmpleadoPeticion(string Documento, string Nombres, string Apellidos,
    string Correo, string Cargo, string AreaOrganizacional);

public sealed class FiltroEmpleadosPeticion
{
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 20;
}
