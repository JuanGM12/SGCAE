using SGCAE.Aplicacion.Abstracciones;
using SGCAE.Dominio.Accesos;
using SGCAE.Dominio.Compartido;
using SGCAE.Dominio.Credenciales;
using SGCAE.Dominio.Empleados;
using SGCAE.Dominio.PuntosAcceso;

namespace SGCAE.Aplicacion.Accesos;

public sealed record MovimientoAccesoDto(Guid Id, Guid? EmpleadoId, Guid? CredencialId,
    Guid? PuntoAccesoId, string CodigoCredencial, string CodigoPuntoAcceso, TipoMovimiento Tipo,
    ResultadoAcceso Resultado, string Motivo, DateTime FechaUtc)
{
    public static MovimientoAccesoDto Desde(MovimientoAcceso m) => new(m.Id, m.EmpleadoId, m.CredencialId,
        m.PuntoAccesoId, m.CodigoCredencial, m.CodigoPuntoAcceso, m.Tipo, m.Resultado, m.Motivo, m.FechaUtc);
}

public sealed record RegistrarAccesoComando(string CodigoCredencial, string CodigoPuntoAcceso, TipoMovimiento Tipo);
public sealed record ConsultarHistorialConsulta(Guid? EmpleadoId = null, DateTime? DesdeUtc = null,
    DateTime? HastaUtc = null, string? CodigoPuntoAcceso = null,
    ResultadoAcceso? Resultado = null, int Pagina = 1, int TamanoPagina = 20);

public sealed class RegistrarAccesoManejador(IRepositorioCredenciales credenciales,
    IRepositorioEmpleados empleados, IRepositorioPuntosAcceso puntos,
    IRepositorioMovimientos movimientos, IUnidadTrabajo uow)
    : IComandoManejador<RegistrarAccesoComando, MovimientoAccesoDto>
{
    public async Task<MovimientoAccesoDto> Ejecutar(RegistrarAccesoComando c, CancellationToken ct = default)
    {
        // Validaciones estructurales: código y movimiento exigidos por la API.
        var codigoCredencial = CodigoCredencial.Crear(c.CodigoCredencial).Valor;
        var codigoPunto = PuntoAcceso.NormalizarCodigo(c.CodigoPuntoAcceso);
        if (!Enum.IsDefined(c.Tipo)) throw new ReglaNegocioException("Movimiento inválido: use Entrada o Salida.");

        // Buscar las referencias antes de decidir; así los rechazos conservan IDs si existen.
        var credencial = await credenciales.ObtenerPorCodigoAsync(codigoCredencial, ct);
        var punto = await puntos.ObtenerPorCodigoAsync(codigoPunto, ct);
        var empleado = credencial is null ? null : await empleados.ObtenerAsync(credencial.EmpleadoId, ct);

        string? rechazo = null;
        if (credencial is null) rechazo = "Credencial no encontrada.";
        else if (empleado is null) rechazo = "Empleado asociado no encontrado.";
        else if (empleado.Estado != EstadoLaboral.Activo) rechazo = "Empleado inactivo.";
        else if (credencial.Estado != EstadoCredencial.Vigente) rechazo = $"Credencial {credencial.Estado.ToString().ToLowerInvariant()}.";
        else if (punto is null) rechazo = "Punto de acceso no encontrado.";
        else if (!punto.Habilitado) rechazo = "Punto de acceso deshabilitado.";
        else
        {
            // Previene movimientos repetidos consecutivos en un mismo punto.
            var ultimo = await movimientos.UltimoAutorizadoAsync(empleado.Id, punto.Id, ct);
            if (ultimo?.Tipo == c.Tipo) rechazo = "Movimiento consecutivo duplicado en este punto.";
        }

        var resultado = rechazo is null ? ResultadoAcceso.Autorizado : ResultadoAcceso.Rechazado;
        var movimiento = MovimientoAcceso.Registrar(codigoCredencial, codigoPunto, c.Tipo, resultado,
            rechazo ?? "Acceso autorizado.", empleado?.Id, credencial?.Id, punto?.Id);
        await movimientos.AgregarAsync(movimiento, ct);
        await uow.GuardarAsync(ct); // También se guardan los rechazos.
        return MovimientoAccesoDto.Desde(movimiento);
    }
}

public sealed class ConsultarHistorialManejador(IRepositorioMovimientos repositorio)
    : IConsultaManejador<ConsultarHistorialConsulta, Pagina<MovimientoAccesoDto>>
{
    public async Task<Pagina<MovimientoAccesoDto>> Ejecutar(ConsultarHistorialConsulta c, CancellationToken ct = default)
    {
        Paginacion.Validar(c.Pagina, c.TamanoPagina);
        if (c.DesdeUtc.HasValue && c.HastaUtc.HasValue && c.DesdeUtc > c.HastaUtc)
            throw new ArgumentException("DesdeUtc no puede ser posterior a HastaUtc.");
        if (c.Resultado.HasValue && !Enum.IsDefined(c.Resultado.Value))
            throw new ArgumentException("Resultado inválido.");
        var filtro = new FiltrosMovimientos(c.EmpleadoId, c.DesdeUtc?.ToUniversalTime(),
            c.HastaUtc?.ToUniversalTime(),
            string.IsNullOrWhiteSpace(c.CodigoPuntoAcceso) ? null : PuntoAcceso.NormalizarCodigo(c.CodigoPuntoAcceso),
            c.Resultado, c.Pagina, c.TamanoPagina);
        var (items, total) = await repositorio.ConsultarAsync(filtro, ct);
        return new Pagina<MovimientoAccesoDto>(items.Select(MovimientoAccesoDto.Desde).ToList(),
            total, c.Pagina, c.TamanoPagina);
    }
}
