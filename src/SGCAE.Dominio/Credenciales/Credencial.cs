using SGCAE.Dominio.Compartido;

namespace SGCAE.Dominio.Credenciales;

public enum EstadoCredencial { Vigente = 1, Bloqueada = 2, Revocada = 3 }

public readonly record struct CodigoCredencial
{
    public string Valor { get; }
    private CodigoCredencial(string valor) => Valor = valor;
    public static CodigoCredencial Crear(string? valor) =>
        new(TextoObligatorio.Validar(valor, "El código de credencial", 50).ToUpperInvariant());
    public override string ToString() => Valor;
}

public sealed class Credencial
{
    public Guid Id { get; private set; }
    public Guid EmpleadoId { get; private set; }
    public CodigoCredencial Codigo { get; private set; }
    public EstadoCredencial Estado { get; private set; }
    public DateTime FechaAsignacionUtc { get; private set; }

    private Credencial() { }

    public static Credencial Asignar(Guid empleadoId, string codigo)
    {
        if (empleadoId == Guid.Empty) throw new ReglaNegocioException("Empleado inválido.");
        return new Credencial
        {
            Id = Guid.NewGuid(), EmpleadoId = empleadoId, Codigo = CodigoCredencial.Crear(codigo),
            Estado = EstadoCredencial.Vigente, FechaAsignacionUtc = DateTime.UtcNow
        };
    }

    public void Bloquear()
    {
        if (Estado == EstadoCredencial.Revocada)
            throw new ReglaNegocioException("No se puede bloquear una credencial revocada.");
        Estado = EstadoCredencial.Bloqueada;
    }

    public void Revocar() => Estado = EstadoCredencial.Revocada;
}
