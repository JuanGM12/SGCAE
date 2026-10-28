using SGCAE.Dominio.Accesos;
using SGCAE.Dominio.Credenciales;
using SGCAE.Dominio.Empleados;
using SGCAE.Dominio.PuntosAcceso;

namespace SGCAE.Aplicacion.Abstracciones;

// CQRS: contratos separados para operaciones que modifican estado y operaciones de lectura.
public interface IComandoManejador<in TComando, TResult>
{
    Task<TResult> Ejecutar(TComando comando, CancellationToken ct = default);
}

public interface IConsultaManejador<in TConsulta, TResult>
{
    Task<TResult> Ejecutar(TConsulta consulta, CancellationToken ct = default);
}

public interface IUnidadTrabajo
{
    Task<int> GuardarAsync(CancellationToken ct = default);
}

public interface IRepositorioEmpleados
{
    Task<Empleado?> ObtenerAsync(Guid id, CancellationToken ct);
    Task<bool> ExisteDocumentoAsync(string documento, Guid? excluirId, CancellationToken ct);
    Task AgregarAsync(Empleado empleado, CancellationToken ct);
    Task<(IReadOnlyList<Empleado> Elementos, int Total)> ListarAsync(int pagina, int tamano, CancellationToken ct);
}

public interface IRepositorioCredenciales
{
    Task<Credencial?> ObtenerAsync(Guid id, CancellationToken ct);
    Task<Credencial?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct);
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct);
    Task<bool> TieneVigenteAsync(Guid empleadoId, CancellationToken ct);
    Task AgregarAsync(Credencial credencial, CancellationToken ct);
    Task<IReadOnlyList<Credencial>> ListarPorEmpleadoAsync(Guid empleadoId, CancellationToken ct);
}

public interface IRepositorioPuntosAcceso
{
    Task<PuntoAcceso?> ObtenerAsync(Guid id, CancellationToken ct);
    Task<PuntoAcceso?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct);
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct);
    Task AgregarAsync(PuntoAcceso punto, CancellationToken ct);
    Task<IReadOnlyList<PuntoAcceso>> ListarAsync(CancellationToken ct);
}

public sealed record FiltrosMovimientos(
    Guid? EmpleadoId, DateTime? DesdeUtc, DateTime? HastaUtc,
    string? CodigoPunto, ResultadoAcceso? Resultado, int Pagina, int TamanoPagina);

public interface IRepositorioMovimientos
{
    Task AgregarAsync(MovimientoAcceso movimiento, CancellationToken ct);
    Task<MovimientoAcceso?> UltimoAutorizadoAsync(Guid empleadoId, Guid puntoAccesoId, CancellationToken ct);
    Task<(IReadOnlyList<MovimientoAcceso> Elementos, int Total)> ConsultarAsync(FiltrosMovimientos filtros, CancellationToken ct);
}

public sealed record Pagina<T>(IReadOnlyList<T> Elementos, int Total, int PaginaActual, int TamanoPagina)
{
    public int TotalPaginas => (int)Math.Ceiling((double)Total / TamanoPagina);
}

public static class Paginacion
{
    public static void Validar(int pagina, int tamano)
    {
        if (pagina < 1) throw new ArgumentException("La página debe ser mayor o igual a 1.");
        if (tamano < 1 || tamano > 100) throw new ArgumentException("El tamaño de página debe estar entre 1 y 100.");
    }
}
