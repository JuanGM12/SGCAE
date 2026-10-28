using SGCAE.Aplicacion.Abstracciones;
using SGCAE.Dominio.Compartido;
using SGCAE.Dominio.Credenciales;
using SGCAE.Dominio.Empleados;

namespace SGCAE.Aplicacion.Credenciales;

public sealed record CredencialDto(Guid Id, Guid EmpleadoId, string Codigo, EstadoCredencial Estado,
    DateTime FechaAsignacionUtc)
{
    public static CredencialDto Desde(Credencial c) =>
        new(c.Id, c.EmpleadoId, c.Codigo.Valor, c.Estado, c.FechaAsignacionUtc);
}

public sealed record AsignarCredencialComando(Guid EmpleadoId, string Codigo);
public sealed record CambiarEstadoCredencialComando(Guid Id, EstadoCredencial Estado);
public sealed record ObtenerCredencialConsulta(Guid Id);
public sealed record ListarCredencialesEmpleadoConsulta(Guid EmpleadoId);

public sealed class AsignarCredencialManejador(IRepositorioCredenciales repositorio,
    IRepositorioEmpleados empleados, IUnidadTrabajo uow)
    : IComandoManejador<AsignarCredencialComando, CredencialDto>
{
    public async Task<CredencialDto> Ejecutar(AsignarCredencialComando c, CancellationToken ct = default)
    {
        var empleado = await empleados.ObtenerAsync(c.EmpleadoId, ct)
            ?? throw new KeyNotFoundException("Empleado no encontrado.");
        if (empleado.Estado != EstadoLaboral.Activo)
            throw new ReglaNegocioException("No se pueden asignar credenciales a empleados inactivos.");
        var credencial = Credencial.Asignar(empleado.Id, c.Codigo);
        if (await repositorio.ExisteCodigoAsync(credencial.Codigo.Valor, ct))
            throw new ReglaNegocioException("El código de credencial ya existe.");
        if (await repositorio.TieneVigenteAsync(empleado.Id, ct))
            throw new ReglaNegocioException("El empleado ya tiene una credencial vigente.");
        await repositorio.AgregarAsync(credencial, ct);
        await uow.GuardarAsync(ct);
        return CredencialDto.Desde(credencial);
    }
}

public sealed class CambiarEstadoCredencialManejador(IRepositorioCredenciales repositorio, IUnidadTrabajo uow)
    : IComandoManejador<CambiarEstadoCredencialComando, CredencialDto>
{
    public async Task<CredencialDto> Ejecutar(CambiarEstadoCredencialComando c, CancellationToken ct = default)
    {
        var credencial = await repositorio.ObtenerAsync(c.Id, ct)
            ?? throw new KeyNotFoundException("Credencial no encontrada.");
        switch (c.Estado)
        {
            case EstadoCredencial.Bloqueada: credencial.Bloquear(); break;
            case EstadoCredencial.Revocada: credencial.Revocar(); break;
            default: throw new ReglaNegocioException("Solo puede bloquear o revocar una credencial desde este endpoint.");
        }
        await uow.GuardarAsync(ct);
        return CredencialDto.Desde(credencial);
    }
}

public sealed class ObtenerCredencialManejador(IRepositorioCredenciales repositorio)
    : IConsultaManejador<ObtenerCredencialConsulta, CredencialDto>
{
    public async Task<CredencialDto> Ejecutar(ObtenerCredencialConsulta c, CancellationToken ct = default) =>
        CredencialDto.Desde(await repositorio.ObtenerAsync(c.Id, ct)
            ?? throw new KeyNotFoundException("Credencial no encontrada."));
}

public sealed class ListarCredencialesEmpleadoManejador(IRepositorioCredenciales repositorio,
    IRepositorioEmpleados empleados)
    : IConsultaManejador<ListarCredencialesEmpleadoConsulta, IReadOnlyList<CredencialDto>>
{
    public async Task<IReadOnlyList<CredencialDto>> Ejecutar(ListarCredencialesEmpleadoConsulta c, CancellationToken ct = default)
    {
        _ = await empleados.ObtenerAsync(c.EmpleadoId, ct)
            ?? throw new KeyNotFoundException("Empleado no encontrado.");
        return (await repositorio.ListarPorEmpleadoAsync(c.EmpleadoId, ct))
            .Select(CredencialDto.Desde).ToList();
    }
}
