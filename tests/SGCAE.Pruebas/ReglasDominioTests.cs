using SGCAE.Dominio.Accesos;
using SGCAE.Dominio.Compartido;
using SGCAE.Dominio.Credenciales;
using SGCAE.Dominio.Empleados;
using SGCAE.Dominio.PuntosAcceso;
using Xunit;

namespace SGCAE.Pruebas;

public sealed class ReglasDominioTests
{
    private static Empleado NuevoEmpleado() => Empleado.Registrar("100100200", "Ana", "Perez",
        "ANA@EJEMPLO.COM", "Operaria", "Bodega");

    [Fact]
    public void Nuevo_empleado_debe_estar_activo_y_con_correo_normalizado()
    {
        var e = NuevoEmpleado();
        Assert.Equal(EstadoLaboral.Activo, e.Estado);
        Assert.Equal("ana@ejemplo.com", e.Correo.Valor);
        Assert.NotEqual(Guid.Empty, e.Id);
    }

    [Fact]
    public void Un_empleado_puede_desactivarse()
    {
        var e = NuevoEmpleado();
        e.Desactivar();
        Assert.Equal(EstadoLaboral.Inactivo, e.Estado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("correo-invalido")]
    [InlineData("ana@")]
    public void Un_correo_invalido_se_rechaza(string correo) =>
        Assert.ThrowsAny<Exception>(() => Empleado.Registrar("123", "Ana", "Perez", correo, "Auxiliar", "RH"));

    [Fact]
    public void No_se_puede_bloquear_una_credencial_revocada()
    {
        var c = Credencial.Asignar(Guid.NewGuid(), "abc-123");
        Assert.Equal("ABC-123", c.Codigo.Valor);
        c.Revocar();
        Assert.Throws<ReglaNegocioException>(() => c.Bloquear());
    }

    [Fact]
    public void Credencial_bloqueada_no_esta_vigente()
    {
        var c = Credencial.Asignar(Guid.NewGuid(), "001");
        c.Bloquear();
        Assert.Equal(EstadoCredencial.Bloqueada, c.Estado);
    }

    [Fact]
    public void Punto_deshabilitado_se_puede_habilitar()
    {
        var p = PuntoAcceso.Registrar("recepcion", "Recepción", "Piso 1", false);
        Assert.Equal("RECEPCION", p.Codigo);
        p.CambiarEstado(true);
        Assert.True(p.Habilitado);
    }

    [Fact]
    public void Movimiento_registra_resultado_y_fecha_utc()
    {
        var m = MovimientoAcceso.Registrar("001", "RECEPCION", TipoMovimiento.Entrada,
            ResultadoAcceso.Rechazado, "Empleado inactivo.", null, null, null);
        Assert.Equal(ResultadoAcceso.Rechazado, m.Resultado);
        Assert.Equal(DateTimeKind.Utc, m.FechaUtc.Kind);
        Assert.Null(m.EmpleadoId);
    }

    [Fact]
    public void Movimiento_con_tipo_desconocido_no_se_registra() =>
        Assert.Throws<ReglaNegocioException>(() => MovimientoAcceso.Registrar(
            "001", "RECEPCION", (TipoMovimiento)99, ResultadoAcceso.Autorizado,
            "Autorizado", null, null, null));
}
