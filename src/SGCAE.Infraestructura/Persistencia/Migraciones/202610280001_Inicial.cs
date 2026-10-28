using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SGCAE.Infraestructura.Persistencia.Migraciones;

[DbContext(typeof(SgcaeDbContext))]
[Migration("202610280001_Inicial")]
public sealed class Inicial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Empleados",
            columns: t => new
            {
                Id = t.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Documento = t.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Nombres = t.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Apellidos = t.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Correo = t.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                Cargo = t.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                AreaOrganizacional = t.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Estado = t.Column<int>(type: "int", nullable: false),
                FechaCreacionUtc = t.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: t => t.PrimaryKey("PK_Empleados", x => x.Id));

        migrationBuilder.CreateTable(
            name: "PuntosAcceso",
            columns: t => new
            {
                Id = t.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Codigo = t.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Nombre = t.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Ubicacion = t.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Habilitado = t.Column<bool>(type: "bit", nullable: false)
            },
            constraints: t => t.PrimaryKey("PK_PuntosAcceso", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Credenciales",
            columns: t => new
            {
                Id = t.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmpleadoId = t.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Codigo = t.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Estado = t.Column<int>(type: "int", nullable: false),
                FechaAsignacionUtc = t.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_Credenciales", x => x.Id);
                t.ForeignKey("FK_Credenciales_Empleados_EmpleadoId", x => x.EmpleadoId,
                    "Empleados", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "MovimientosAcceso",
            columns: t => new
            {
                Id = t.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmpleadoId = t.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CredencialId = t.Column<Guid>(type: "uniqueidentifier", nullable: true),
                PuntoAccesoId = t.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CodigoCredencial = t.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                CodigoPuntoAcceso = t.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Tipo = t.Column<int>(type: "int", nullable: false),
                Resultado = t.Column<int>(type: "int", nullable: false),
                Motivo = t.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                FechaUtc = t.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_MovimientosAcceso", x => x.Id);
                t.ForeignKey("FK_MovimientosAcceso_Empleados_EmpleadoId", x => x.EmpleadoId,
                    "Empleados", "Id", onDelete: ReferentialAction.Restrict);
                t.ForeignKey("FK_MovimientosAcceso_Credenciales_CredencialId", x => x.CredencialId,
                    "Credenciales", "Id", onDelete: ReferentialAction.Restrict);
                t.ForeignKey("FK_MovimientosAcceso_PuntosAcceso_PuntoAccesoId", x => x.PuntoAccesoId,
                    "PuntosAcceso", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_Empleados_Documento", "Empleados", "Documento", unique: true);
        migrationBuilder.CreateIndex("IX_Credenciales_Codigo", "Credenciales", "Codigo", unique: true);
        migrationBuilder.CreateIndex("IX_Credenciales_EmpleadoId", "Credenciales", "EmpleadoId");
        migrationBuilder.CreateIndex("IX_PuntosAcceso_Codigo", "PuntosAcceso", "Codigo", unique: true);
        migrationBuilder.CreateIndex("IX_MovimientosAcceso_EmpleadoId_FechaUtc", "MovimientosAcceso", new[] { "EmpleadoId", "FechaUtc" });
        migrationBuilder.CreateIndex("IX_MovimientosAcceso_CodigoPuntoAcceso_FechaUtc", "MovimientosAcceso", new[] { "CodigoPuntoAcceso", "FechaUtc" });
        migrationBuilder.CreateIndex("IX_MovimientosAcceso_Resultado_FechaUtc", "MovimientosAcceso", new[] { "Resultado", "FechaUtc" });
        migrationBuilder.CreateIndex("IX_MovimientosAcceso_CredencialId", "MovimientosAcceso", "CredencialId");
        migrationBuilder.CreateIndex("IX_MovimientosAcceso_PuntoAccesoId", "MovimientosAcceso", "PuntoAccesoId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "MovimientosAcceso");
        migrationBuilder.DropTable(name: "Credenciales");
        migrationBuilder.DropTable(name: "PuntosAcceso");
        migrationBuilder.DropTable(name: "Empleados");
    }
}
