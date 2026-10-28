using SGCAE.Aplicacion.Abstracciones;
using SGCAE.Dominio.Compartido;
using SGCAE.Dominio.PuntosAcceso;

namespace SGCAE.Aplicacion.PuntosAcceso;

public sealed record PuntoAccesoDto(Guid Id, string Codigo, string Nombre, string Ubicacion, bool Habilitado)
{
    public static PuntoAccesoDto Desde(PuntoAcceso p) => new(p.Id, p.Codigo, p.Nombre, p.Ubicacion, p.Habilitado);
}

public sealed record RegistrarPuntoAccesoComando(string Codigo, string Nombre, string Ubicacion, bool Habilitado);
public sealed record CambiarEstadoPuntoAccesoComando(Guid Id, bool Habilitado);
public sealed record ObtenerPuntoAccesoConsulta(Guid Id);
public sealed record ListarPuntosAccesoConsulta();

public sealed class RegistrarPuntoAccesoManejador(IRepositorioPuntosAcceso repositorio, IUnidadTrabajo uow)
    : IComandoManejador<RegistrarPuntoAccesoComando, PuntoAccesoDto>
{
    public async Task<PuntoAccesoDto> Ejecutar(RegistrarPuntoAccesoComando c, CancellationToken ct = default)
    {
        var punto = PuntoAcceso.Registrar(c.Codigo, c.Nombre, c.Ubicacion, c.Habilitado);
        if (await repositorio.ExisteCodigoAsync(punto.Codigo, ct))
            throw new ReglaNegocioException("Ya existe un punto de acceso con este código.");
        await repositorio.AgregarAsync(punto, ct);
        await uow.GuardarAsync(ct);
        return PuntoAccesoDto.Desde(punto);
    }
}

public sealed class CambiarEstadoPuntoAccesoManejador(IRepositorioPuntosAcceso repositorio, IUnidadTrabajo uow)
    : IComandoManejador<CambiarEstadoPuntoAccesoComando, PuntoAccesoDto>
{
    public async Task<PuntoAccesoDto> Ejecutar(CambiarEstadoPuntoAccesoComando c, CancellationToken ct = default)
    {
        var punto = await repositorio.ObtenerAsync(c.Id, ct)
            ?? throw new KeyNotFoundException("Punto de acceso no encontrado.");
        punto.CambiarEstado(c.Habilitado);
        await uow.GuardarAsync(ct);
        return PuntoAccesoDto.Desde(punto);
    }
}

public sealed class ObtenerPuntoAccesoManejador(IRepositorioPuntosAcceso repositorio)
    : IConsultaManejador<ObtenerPuntoAccesoConsulta, PuntoAccesoDto>
{
    public async Task<PuntoAccesoDto> Ejecutar(ObtenerPuntoAccesoConsulta c, CancellationToken ct = default) =>
        PuntoAccesoDto.Desde(await repositorio.ObtenerAsync(c.Id, ct)
            ?? throw new KeyNotFoundException("Punto de acceso no encontrado."));
}

public sealed class ListarPuntosAccesoManejador(IRepositorioPuntosAcceso repositorio)
    : IConsultaManejador<ListarPuntosAccesoConsulta, IReadOnlyList<PuntoAccesoDto>>
{
    public async Task<IReadOnlyList<PuntoAccesoDto>> Ejecutar(ListarPuntosAccesoConsulta c, CancellationToken ct = default) =>
        (await repositorio.ListarAsync(ct)).Select(PuntoAccesoDto.Desde).ToList();
}
