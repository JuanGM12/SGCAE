using SGCAE.Dominio.Compartido;
using SGCAE.Dominio.Credenciales;
using SGCAE.Dominio.PuntosAcceso;

namespace SGCAE.Dominio.Accesos;

public enum TipoMovimiento { Entrada = 1, Salida = 2 }
public enum ResultadoAcceso { Autorizado = 1, Rechazado = 2 }

// Registro inmutable desde la aplicación: incluso los rechazos se conservan para auditoría.
public sealed class MovimientoAcceso
{
    public Guid Id { get; private set; }
    public Guid? EmpleadoId { get; private set; }
    public Guid? CredencialId { get; private set; }
    public Guid? PuntoAccesoId { get; private set; }
    public string CodigoCredencial { get; private set; } = null!;
    public string CodigoPuntoAcceso { get; private set; } = null!;
    public TipoMovimiento Tipo { get; private set; }
    public ResultadoAcceso Resultado { get; private set; }
    public string Motivo { get; private set; } = null!;
    public DateTime FechaUtc { get; private set; }

    private MovimientoAcceso() { }

    public static MovimientoAcceso Registrar(string codigoCredencial, string codigoPunto,
        TipoMovimiento tipo, ResultadoAcceso resultado, string motivo,
        Guid? empleadoId, Guid? credencialId, Guid? puntoAccesoId)
    {
        if (!Enum.IsDefined(tipo)) throw new ReglaNegocioException("Tipo de movimiento no válido.");
        if (!Enum.IsDefined(resultado)) throw new ReglaNegocioException("Resultado no válido.");
        return new MovimientoAcceso
        {
            Id = Guid.NewGuid(), CodigoCredencial = global::SGCAE.Dominio.Credenciales.CodigoCredencial.Crear(codigoCredencial).Valor,
            CodigoPuntoAcceso = PuntoAcceso.NormalizarCodigo(codigoPunto),
            Tipo = tipo, Resultado = resultado,
            Motivo = TextoObligatorio.Validar(motivo, "El motivo", 250),
            EmpleadoId = empleadoId, CredencialId = credencialId, PuntoAccesoId = puntoAccesoId,
            FechaUtc = DateTime.UtcNow
        };
    }
}
