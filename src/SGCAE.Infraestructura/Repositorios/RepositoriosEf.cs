using Microsoft.EntityFrameworkCore;
using SGCAE.Aplicacion.Abstracciones;
using SGCAE.Dominio.Accesos;
using SGCAE.Dominio.Credenciales;
using SGCAE.Dominio.Empleados;
using SGCAE.Dominio.PuntosAcceso;
using SGCAE.Infraestructura.Persistencia;

namespace SGCAE.Infraestructura.Repositorios;

public sealed class RepositorioEmpleados(SgcaeDbContext db) : IRepositorioEmpleados
{
    public Task<Empleado?> ObtenerAsync(Guid id, CancellationToken ct) =>
        db.Empleados.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> ExisteDocumentoAsync(string documento, Guid? excluirId, CancellationToken ct) =>
        db.Empleados.AnyAsync(e => e.Documento == documento && (!excluirId.HasValue || e.Id != excluirId.Value), ct);

    public async Task AgregarAsync(Empleado empleado, CancellationToken ct) =>
        await db.Empleados.AddAsync(empleado, ct);

    public async Task<(IReadOnlyList<Empleado> Elementos, int Total)> ListarAsync(int pagina, int tamano, CancellationToken ct)
    {
        var query = db.Empleados.AsNoTracking();
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(e => e.Apellidos).ThenBy(e => e.Nombres)
            .Skip((pagina - 1) * tamano).Take(tamano).ToListAsync(ct);
        return (items, total);
    }
}

public sealed class RepositorioCredenciales(SgcaeDbContext db) : IRepositorioCredenciales
{
    public Task<Credencial?> ObtenerAsync(Guid id, CancellationToken ct) =>
        db.Credenciales.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Credencial?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct) =>
        db.Credenciales.FirstOrDefaultAsync(c => c.Codigo == CodigoCredencial.Crear(codigo), ct);

    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct) =>
        db.Credenciales.AnyAsync(c => c.Codigo == CodigoCredencial.Crear(codigo), ct);

    public Task<bool> TieneVigenteAsync(Guid empleadoId, CancellationToken ct) =>
        db.Credenciales.AnyAsync(c => c.EmpleadoId == empleadoId && c.Estado == EstadoCredencial.Vigente, ct);

    public async Task AgregarAsync(Credencial credencial, CancellationToken ct) =>
        await db.Credenciales.AddAsync(credencial, ct);

    public async Task<IReadOnlyList<Credencial>> ListarPorEmpleadoAsync(Guid empleadoId, CancellationToken ct) =>
        await db.Credenciales.AsNoTracking().Where(c => c.EmpleadoId == empleadoId)
            .OrderByDescending(c => c.FechaAsignacionUtc).ToListAsync(ct);
}

public sealed class RepositorioPuntosAcceso(SgcaeDbContext db) : IRepositorioPuntosAcceso
{
    public Task<PuntoAcceso?> ObtenerAsync(Guid id, CancellationToken ct) =>
        db.PuntosAcceso.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<PuntoAcceso?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct) =>
        db.PuntosAcceso.FirstOrDefaultAsync(p => p.Codigo == codigo, ct);

    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct) =>
        db.PuntosAcceso.AnyAsync(p => p.Codigo == codigo, ct);

    public async Task AgregarAsync(PuntoAcceso punto, CancellationToken ct) =>
        await db.PuntosAcceso.AddAsync(punto, ct);

    public async Task<IReadOnlyList<PuntoAcceso>> ListarAsync(CancellationToken ct) =>
        await db.PuntosAcceso.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync(ct);
}

public sealed class RepositorioMovimientos(SgcaeDbContext db) : IRepositorioMovimientos
{
    public async Task AgregarAsync(MovimientoAcceso movimiento, CancellationToken ct) =>
        await db.MovimientosAcceso.AddAsync(movimiento, ct);

    public Task<MovimientoAcceso?> UltimoAutorizadoAsync(Guid empleadoId, Guid puntoAccesoId, CancellationToken ct) =>
        db.MovimientosAcceso.AsNoTracking()
            .Where(m => m.EmpleadoId == empleadoId && m.PuntoAccesoId == puntoAccesoId
                && m.Resultado == ResultadoAcceso.Autorizado)
            .OrderByDescending(m => m.FechaUtc).ThenByDescending(m => m.Id)
            .FirstOrDefaultAsync(ct);

    public async Task<(IReadOnlyList<MovimientoAcceso> Elementos, int Total)> ConsultarAsync(
        FiltrosMovimientos f, CancellationToken ct)
    {
        var query = db.MovimientosAcceso.AsNoTracking().AsQueryable();
        if (f.EmpleadoId.HasValue) query = query.Where(m => m.EmpleadoId == f.EmpleadoId.Value);
        if (f.DesdeUtc.HasValue) query = query.Where(m => m.FechaUtc >= f.DesdeUtc.Value);
        if (f.HastaUtc.HasValue) query = query.Where(m => m.FechaUtc <= f.HastaUtc.Value);
        if (f.CodigoPunto is not null) query = query.Where(m => m.CodigoPuntoAcceso == f.CodigoPunto);
        if (f.Resultado.HasValue) query = query.Where(m => m.Resultado == f.Resultado.Value);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(m => m.FechaUtc).ThenByDescending(m => m.Id)
            .Skip((f.Pagina - 1) * f.TamanoPagina).Take(f.TamanoPagina).ToListAsync(ct);
        return (items, total);
    }
}
