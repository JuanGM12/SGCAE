using SGCAE.Aplicacion.Abstracciones;
using SGCAE.Aplicacion.Empleados;
using SGCAE.Dominio.Empleados;
using SGCAE.Dominio.Compartido;
using Xunit;

namespace SGCAE.Pruebas;

public sealed class EmpleadosTests
{
    private sealed class RepositorioFalso : IRepositorioEmpleados, IUnidadTrabajo
    {
        public List<Empleado> Items { get; } = [];
        public Task<Empleado?> ObtenerAsync(Guid id, CancellationToken ct) => Task.FromResult(Items.FirstOrDefault(x => x.Id == id));
        public Task<bool> ExisteDocumentoAsync(string doc, Guid? except, CancellationToken ct) =>
            Task.FromResult(Items.Any(x => x.Documento == doc && x.Id != except));
        public Task AgregarAsync(Empleado empleado, CancellationToken ct) { Items.Add(empleado); return Task.CompletedTask; }
        public Task<(IReadOnlyList<Empleado> Elementos, int Total)> ListarAsync(int pagina, int tamano, CancellationToken ct) =>
            Task.FromResult<(IReadOnlyList<Empleado>, int)>((Items.Skip((pagina - 1) * tamano).Take(tamano).ToList(), Items.Count));
        public Task<int> GuardarAsync(CancellationToken ct = default) => Task.FromResult(1);
    }

    [Fact]
    public async Task Crear_empleado_y_evitar_documento_duplicado()
    {
        var repo = new RepositorioFalso();
        var manejador = new CrearEmpleadoManejador(repo, repo);
        var comando = new CrearEmpleadoComando("999111", "Luis", "Lopez", "luis@empresa.com", "Supervisor", "Bodega");
        var empleado = await manejador.Ejecutar(comando);
        Assert.Equal("999111", empleado.Documento);
        await Assert.ThrowsAsync<ReglaNegocioException>(() => manejador.Ejecutar(comando));
    }

    [Fact]
    public async Task Desactivar_empleado_conserva_datos()
    {
        var repo = new RepositorioFalso();
        var creado = await new CrearEmpleadoManejador(repo, repo).Ejecutar(
            new("101", "Luis", "Lopez", "luis@empresa.com", "Operario", "Bodega"));
        var desactivado = await new DesactivarEmpleadoManejador(repo, repo).Ejecutar(new(creado.Id));
        Assert.Equal(EstadoLaboral.Inactivo, desactivado.Estado);
        Assert.Equal("101", desactivado.Documento);
    }
}
