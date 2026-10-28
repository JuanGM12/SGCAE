using Microsoft.EntityFrameworkCore;
using SGCAE.Aplicacion.Abstracciones;
using SGCAE.Dominio.Accesos;
using SGCAE.Dominio.Credenciales;
using SGCAE.Dominio.Empleados;
using SGCAE.Dominio.PuntosAcceso;

namespace SGCAE.Infraestructura.Persistencia;

public sealed class SgcaeDbContext(DbContextOptions<SgcaeDbContext> options) : DbContext(options), IUnidadTrabajo
{
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Credencial> Credenciales => Set<Credencial>();
    public DbSet<PuntoAcceso> PuntosAcceso => Set<PuntoAcceso>();
    public DbSet<MovimientoAcceso> MovimientosAcceso => Set<MovimientoAcceso>();

    public Task<int> GuardarAsync(CancellationToken ct = default) => SaveChangesAsync(ct);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empleado>(entidad =>
        {
            entidad.ToTable("Empleados");
            entidad.HasKey(e => e.Id);
            entidad.Property(e => e.Documento).HasMaxLength(30).IsRequired();
            entidad.HasIndex(e => e.Documento).IsUnique();
            entidad.Property(e => e.Nombres).HasMaxLength(100).IsRequired();
            entidad.Property(e => e.Apellidos).HasMaxLength(100).IsRequired();
            entidad.Property(e => e.Correo).HasConversion(c => c.Valor, v => CorreoElectronico.Crear(v))
                .HasColumnName("Correo").HasMaxLength(254).IsRequired();
            entidad.Property(e => e.Cargo).HasMaxLength(120).IsRequired();
            entidad.Property(e => e.AreaOrganizacional).HasMaxLength(120).IsRequired();
            entidad.Property(e => e.Estado).HasConversion<int>().IsRequired();
            entidad.Property(e => e.FechaCreacionUtc)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                .HasColumnType("datetime2").IsRequired();
        });

        modelBuilder.Entity<Credencial>(entidad =>
        {
            entidad.ToTable("Credenciales");
            entidad.HasKey(c => c.Id);
            entidad.Property(c => c.Codigo).HasConversion(c => c.Valor, v => CodigoCredencial.Crear(v))
                .HasMaxLength(50).IsRequired();
            entidad.HasIndex(c => c.Codigo).IsUnique();
            entidad.Property(c => c.Estado).HasConversion<int>().IsRequired();
            entidad.Property(c => c.FechaAsignacionUtc)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                .HasColumnType("datetime2").IsRequired();
            entidad.HasIndex(c => c.EmpleadoId);
            entidad.HasOne<Empleado>().WithMany().HasForeignKey(c => c.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PuntoAcceso>(entidad =>
        {
            entidad.ToTable("PuntosAcceso");
            entidad.HasKey(p => p.Id);
            entidad.Property(p => p.Codigo).HasMaxLength(30).IsRequired();
            entidad.HasIndex(p => p.Codigo).IsUnique();
            entidad.Property(p => p.Nombre).HasMaxLength(120).IsRequired();
            entidad.Property(p => p.Ubicacion).HasMaxLength(150).IsRequired();
            entidad.Property(p => p.Habilitado).IsRequired();
        });

        modelBuilder.Entity<MovimientoAcceso>(entidad =>
        {
            entidad.ToTable("MovimientosAcceso");
            entidad.HasKey(m => m.Id);
            entidad.Property(m => m.CodigoCredencial).HasMaxLength(50).IsRequired();
            entidad.Property(m => m.CodigoPuntoAcceso).HasMaxLength(30).IsRequired();
            entidad.Property(m => m.Tipo).HasConversion<int>().IsRequired();
            entidad.Property(m => m.Resultado).HasConversion<int>().IsRequired();
            entidad.Property(m => m.Motivo).HasMaxLength(250).IsRequired();
            entidad.Property(m => m.FechaUtc)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                .HasColumnType("datetime2").IsRequired();
            entidad.HasOne<Empleado>().WithMany().HasForeignKey(m => m.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);
            entidad.HasOne<Credencial>().WithMany().HasForeignKey(m => m.CredencialId)
                .OnDelete(DeleteBehavior.Restrict);
            entidad.HasOne<PuntoAcceso>().WithMany().HasForeignKey(m => m.PuntoAccesoId)
                .OnDelete(DeleteBehavior.Restrict);
            entidad.HasIndex(m => new { m.EmpleadoId, m.FechaUtc });
            entidad.HasIndex(m => new { m.CodigoPuntoAcceso, m.FechaUtc });
            entidad.HasIndex(m => new { m.Resultado, m.FechaUtc });
        });
    }
}
