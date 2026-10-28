using SGCAE.Dominio.Compartido;

namespace SGCAE.Dominio.PuntosAcceso;

public sealed class PuntoAcceso
{
    public Guid Id { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public string Ubicacion { get; private set; } = null!;
    public bool Habilitado { get; private set; }

    private PuntoAcceso() { }

    public static PuntoAcceso Registrar(string codigo, string nombre, string ubicacion, bool habilitado)
    {
        return new PuntoAcceso
        {
            Id = Guid.NewGuid(),
            Codigo = NormalizarCodigo(codigo),
            Nombre = TextoObligatorio.Validar(nombre, "El nombre del punto", 120),
            Ubicacion = TextoObligatorio.Validar(ubicacion, "La ubicación", 150),
            Habilitado = habilitado
        };
    }

    public static string NormalizarCodigo(string? codigo) =>
        TextoObligatorio.Validar(codigo, "El código del punto de acceso", 30).ToUpperInvariant();

    public void CambiarEstado(bool habilitado) => Habilitado = habilitado;
}
