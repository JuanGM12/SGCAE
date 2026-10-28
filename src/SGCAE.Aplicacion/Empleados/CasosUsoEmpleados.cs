using SGCAE.Aplicacion.Abstracciones;
using SGCAE.Dominio.Compartido;
using SGCAE.Dominio.Empleados;

namespace SGCAE.Aplicacion.Empleados;

public sealed record EmpleadoDto(Guid Id, string Documento, string Nombres, string Apellidos,
    string Correo, string Cargo, string AreaOrganizacional, EstadoLaboral Estado, DateTime FechaCreacionUtc)
{
    public static EmpleadoDto Desde(Empleado e) => new(e.Id, e.Documento, e.Nombres,
        e.Apellidos, e.Correo.Valor, e.Cargo, e.AreaOrganizacional, e.Estado, e.FechaCreacionUtc);
}

public sealed record CrearEmpleadoComando(string Documento, string Nombres, string Apellidos,
    string Correo, string Cargo, string AreaOrganizacional);
public sealed record ActualizarEmpleadoComando(Guid Id, string Documento, string Nombres,
    string Apellidos, string Correo, string Cargo, string AreaOrganizacional);
public sealed record DesactivarEmpleadoComando(Guid Id);
public sealed record ObtenerEmpleadoConsulta(Guid Id);
public sealed record ListarEmpleadosConsulta(int Pagina = 1, int TamanoPagina = 20);

public sealed class CrearEmpleadoManejador(IRepositorioEmpleados repositorio, IUnidadTrabajo uow)
    : IComandoManejador<CrearEmpleadoComando, EmpleadoDto>
{
    public async Task<EmpleadoDto> Ejecutar(CrearEmpleadoComando c, CancellationToken ct = default)
    {
        var empleado = Empleado.Registrar(c.Documento, c.Nombres, c.Apellidos, c.Correo, c.Cargo, c.AreaOrganizacional);
        if (await repositorio.ExisteDocumentoAsync(empleado.Documento, null, ct))
            throw new ReglaNegocioException("Ya existe un empleado con ese documento.");
        await repositorio.AgregarAsync(empleado, ct);
        await uow.GuardarAsync(ct);
        return EmpleadoDto.Desde(empleado);
    }
}

public sealed class ActualizarEmpleadoManejador(IRepositorioEmpleados repositorio, IUnidadTrabajo uow)
    : IComandoManejador<ActualizarEmpleadoComando, EmpleadoDto>
{
    public async Task<EmpleadoDto> Ejecutar(ActualizarEmpleadoComando c, CancellationToken ct = default)
    {
        var empleado = await repositorio.ObtenerAsync(c.Id, ct)
            ?? throw new KeyNotFoundException("Empleado no encontrado.");
        // Valida las nuevas propiedades de dominio antes de consultar la unicidad.
        empleado.Actualizar(c.Documento, c.Nombres, c.Apellidos, c.Correo, c.Cargo, c.AreaOrganizacional);
        if (await repositorio.ExisteDocumentoAsync(empleado.Documento, empleado.Id, ct))
            throw new ReglaNegocioException("Otro empleado ya utiliza ese documento.");
        await uow.GuardarAsync(ct);
        return EmpleadoDto.Desde(empleado);
    }
}

public sealed class DesactivarEmpleadoManejador(IRepositorioEmpleados repositorio, IUnidadTrabajo uow)
    : IComandoManejador<DesactivarEmpleadoComando, EmpleadoDto>
{
    public async Task<EmpleadoDto> Ejecutar(DesactivarEmpleadoComando c, CancellationToken ct = default)
    {
        var empleado = await repositorio.ObtenerAsync(c.Id, ct)
            ?? throw new KeyNotFoundException("Empleado no encontrado.");
        empleado.Desactivar();
        await uow.GuardarAsync(ct);
        return EmpleadoDto.Desde(empleado);
    }
}

public sealed class ObtenerEmpleadoManejador(IRepositorioEmpleados repositorio)
    : IConsultaManejador<ObtenerEmpleadoConsulta, EmpleadoDto>
{
    public async Task<EmpleadoDto> Ejecutar(ObtenerEmpleadoConsulta c, CancellationToken ct = default) =>
        EmpleadoDto.Desde(await repositorio.ObtenerAsync(c.Id, ct)
            ?? throw new KeyNotFoundException("Empleado no encontrado."));
}

public sealed class ListarEmpleadosManejador(IRepositorioEmpleados repositorio)
    : IConsultaManejador<ListarEmpleadosConsulta, Pagina<EmpleadoDto>>
{
    public async Task<Pagina<EmpleadoDto>> Ejecutar(ListarEmpleadosConsulta c, CancellationToken ct = default)
    {
        Paginacion.Validar(c.Pagina, c.TamanoPagina);
        var (items, total) = await repositorio.ListarAsync(c.Pagina, c.TamanoPagina, ct);
        return new Pagina<EmpleadoDto>(items.Select(EmpleadoDto.Desde).ToList(), total, c.Pagina, c.TamanoPagina);
    }
}
