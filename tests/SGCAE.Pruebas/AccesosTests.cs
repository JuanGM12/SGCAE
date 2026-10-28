using SGCAE.Aplicacion.Abstracciones;
using SGCAE.Aplicacion.Accesos;
using SGCAE.Aplicacion.Credenciales;
using SGCAE.Dominio.Accesos;
using SGCAE.Dominio.Credenciales;
using SGCAE.Dominio.Empleados;
using SGCAE.Dominio.PuntosAcceso;
using Xunit;

namespace SGCAE.Pruebas;

public sealed class AccesosTests
{
    private sealed class DatosPrueba : IRepositorioEmpleados, IRepositorioCredenciales,
        IRepositorioPuntosAcceso, IRepositorioMovimientos, IUnidadTrabajo
    {
        public Empleado Empleado { get; } = SGCAE.Dominio.Empleados.Empleado.Registrar("109900", "Laura", "Garcia",
            "laura@empresa.com", "Analista", "Operaciones");
        public Credencial Credencial { get; private set; }
        public PuntoAcceso Punto { get; } = PuntoAcceso.Registrar("BODEGA", "Bodega", "Zona A", true);
        public List<MovimientoAcceso> Movimientos { get; } = [];
        public DatosPrueba() => Credencial = SGCAE.Dominio.Credenciales.Credencial.Asignar(Empleado.Id, "TARJETA-01");
        Task<Empleado?> IRepositorioEmpleados.ObtenerAsync(Guid id, CancellationToken ct) => Task.FromResult<Empleado?>(id == Empleado.Id ? Empleado : null);
        Task<Credencial?> IRepositorioCredenciales.ObtenerAsync(Guid id, CancellationToken ct) => Task.FromResult<Credencial?>(id == Credencial.Id ? Credencial : null);
        Task<PuntoAcceso?> IRepositorioPuntosAcceso.ObtenerAsync(Guid id, CancellationToken ct) => Task.FromResult<PuntoAcceso?>(id == Punto.Id ? Punto : null);
        Task<Credencial?> IRepositorioCredenciales.ObtenerPorCodigoAsync(string codigo, CancellationToken ct) =>
            Task.FromResult<Credencial?>(Credencial.Codigo.Valor == codigo ? Credencial : null);
        Task<PuntoAcceso?> IRepositorioPuntosAcceso.ObtenerPorCodigoAsync(string codigo, CancellationToken ct) =>
            Task.FromResult<PuntoAcceso?>(Punto.Codigo == codigo ? Punto : null);
        public Task<MovimientoAcceso?> UltimoAutorizadoAsync(Guid empleadoId, Guid puntoAccesoId, CancellationToken ct) =>
            Task.FromResult(Movimientos.Where(m => m.EmpleadoId == empleadoId && m.PuntoAccesoId == puntoAccesoId &&
                m.Resultado == ResultadoAcceso.Autorizado).OrderByDescending(m => m.FechaUtc).FirstOrDefault());
        public Task AgregarAsync(MovimientoAcceso movimiento, CancellationToken ct)
        {
            Movimientos.Add(movimiento);
            return Task.CompletedTask;
        }
        public Task AgregarAsync(Empleado empleado, CancellationToken ct) => Task.CompletedTask;
        public Task AgregarAsync(Credencial credencial, CancellationToken ct) => Task.CompletedTask;
        public Task AgregarAsync(PuntoAcceso punto, CancellationToken ct) => Task.CompletedTask;
        public Task<int> GuardarAsync(CancellationToken ct = default) => Task.FromResult(1);
        public Task<bool> ExisteDocumentoAsync(string documento, Guid? excluirId, CancellationToken ct) => Task.FromResult(false);
        public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct) => Task.FromResult(false);
        public Task<bool> TieneVigenteAsync(Guid empleadoId, CancellationToken ct) => Task.FromResult(false);
        public Task<(IReadOnlyList<Empleado> Elementos, int Total)> ListarAsync(int pagina, int tamano, CancellationToken ct) =>
            Task.FromResult<(IReadOnlyList<Empleado>, int)>((new List<Empleado> { Empleado }, 1));
        public Task<IReadOnlyList<Credencial>> ListarPorEmpleadoAsync(Guid empleadoId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<Credencial>>(new List<Credencial> { Credencial });
        public Task<IReadOnlyList<PuntoAcceso>> ListarAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<PuntoAcceso>>(new List<PuntoAcceso> { Punto });
        public Task<(IReadOnlyList<MovimientoAcceso> Elementos, int Total)> ConsultarAsync(FiltrosMovimientos f, CancellationToken ct)
        {
            var items = Movimientos.AsEnumerable();
            if (f.EmpleadoId.HasValue) items = items.Where(x => x.EmpleadoId == f.EmpleadoId.Value);
            if (f.Resultado.HasValue) items = items.Where(x => x.Resultado == f.Resultado.Value);
            if (f.CodigoPunto is not null) items = items.Where(x => x.CodigoPuntoAcceso == f.CodigoPunto);
            if (f.DesdeUtc.HasValue) items = items.Where(x => x.FechaUtc >= f.DesdeUtc);
            if (f.HastaUtc.HasValue) items = items.Where(x => x.FechaUtc <= f.HastaUtc);
            var all = items.ToList();
            return Task.FromResult<(IReadOnlyList<MovimientoAcceso>, int)>((
                all.Skip((f.Pagina - 1) * f.TamanoPagina).Take(f.TamanoPagina).ToList(), all.Count));
        }
    }

    private static RegistrarAccesoManejador Manejador(DatosPrueba datos) =>
        new(datos, datos, datos, datos, datos);

    [Fact]
    public async Task Entrada_valida_es_autorizada_y_guardada()
    {
        var datos = new DatosPrueba();
        var acceso = await Manejador(datos).Ejecutar(new("TARJETA-01", "BODEGA", TipoMovimiento.Entrada));
        Assert.Equal(ResultadoAcceso.Autorizado, acceso.Resultado);
        Assert.Single(datos.Movimientos);
    }

    [Fact]
    public async Task Credencial_inexistente_es_rechazada_pero_auditada()
    {
        var datos = new DatosPrueba();
        var acceso = await Manejador(datos).Ejecutar(new("DESCONOCIDA", "BODEGA", TipoMovimiento.Entrada));
        Assert.Equal(ResultadoAcceso.Rechazado, acceso.Resultado);
        Assert.Null(acceso.EmpleadoId);
        Assert.Single(datos.Movimientos);
    }

    [Fact]
    public async Task Empleado_inactivo_no_puede_entrar()
    {
        var datos = new DatosPrueba();
        datos.Empleado.Desactivar();
        var acceso = await Manejador(datos).Ejecutar(new("TARJETA-01", "BODEGA", TipoMovimiento.Entrada));
        Assert.Equal(ResultadoAcceso.Rechazado, acceso.Resultado);
        Assert.Contains("inactivo", acceso.Motivo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Credencial_bloqueada_no_autoriza()
    {
        var datos = new DatosPrueba();
        datos.Credencial.Bloquear();
        var acceso = await Manejador(datos).Ejecutar(new("TARJETA-01", "BODEGA", TipoMovimiento.Entrada));
        Assert.Equal(ResultadoAcceso.Rechazado, acceso.Resultado);
    }

    [Fact]
    public async Task Punto_deshabilitado_no_autoriza()
    {
        var datos = new DatosPrueba();
        datos.Punto.CambiarEstado(false);
        var acceso = await Manejador(datos).Ejecutar(new("TARJETA-01", "BODEGA", TipoMovimiento.Entrada));
        Assert.Equal(ResultadoAcceso.Rechazado, acceso.Resultado);
    }

    [Fact]
    public async Task Entrada_repetida_se_rechaza_y_la_salida_se_autoriza()
    {
        var datos = new DatosPrueba();
        var manejador = Manejador(datos);
        var primera = await manejador.Ejecutar(new("TARJETA-01", "BODEGA", TipoMovimiento.Entrada));
        var duplicada = await manejador.Ejecutar(new("TARJETA-01", "BODEGA", TipoMovimiento.Entrada));
        var salida = await manejador.Ejecutar(new("TARJETA-01", "BODEGA", TipoMovimiento.Salida));
        Assert.Equal(ResultadoAcceso.Autorizado, primera.Resultado);
        Assert.Equal(ResultadoAcceso.Rechazado, duplicada.Resultado);
        Assert.Equal(ResultadoAcceso.Autorizado, salida.Resultado);
        Assert.Equal(3, datos.Movimientos.Count);
    }

    [Fact]
    public async Task Historial_filtra_resultado_y_pagina()
    {
        var datos = new DatosPrueba();
        await Manejador(datos).Ejecutar(new("TARJETA-01", "BODEGA", TipoMovimiento.Entrada));
        await Manejador(datos).Ejecutar(new("MALA", "BODEGA", TipoMovimiento.Entrada));
        var consulta = new ConsultarHistorialManejador(datos);
        var pagina = await consulta.Ejecutar(new(Resultado: ResultadoAcceso.Rechazado, Pagina: 1, TamanoPagina: 1));
        Assert.Single(pagina.Elementos);
        Assert.Equal(1, pagina.Total);
        Assert.Equal(ResultadoAcceso.Rechazado, pagina.Elementos[0].Resultado);
    }
}
