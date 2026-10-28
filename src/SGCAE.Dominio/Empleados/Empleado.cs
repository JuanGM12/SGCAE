using System.Net.Mail;
using SGCAE.Dominio.Compartido;

namespace SGCAE.Dominio.Empleados;

public enum EstadoLaboral { Activo = 1, Inactivo = 2 }

// Objeto de valor: evita persistir correos inválidos y normaliza su formato.
public readonly record struct CorreoElectronico
{
    public string Valor { get; }
    private CorreoElectronico(string valor) => Valor = valor;
    public static CorreoElectronico Crear(string? valor)
    {
        var correo = TextoObligatorio.Validar(valor, "El correo electrónico", 254).ToLowerInvariant();
        try
        {
            var parsed = new MailAddress(correo);
            if (parsed.Address != correo || correo.Contains(' ')) throw new FormatException();
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            throw new ReglaNegocioException("El correo electrónico no tiene un formato válido.");
        }
        return new CorreoElectronico(correo);
    }
    public override string ToString() => Valor;
}

// Agregado / entidad de dominio con identidad propia y reglas de negocio.
public sealed class Empleado
{
    public Guid Id { get; private set; }
    public string Documento { get; private set; } = null!;
    public string Nombres { get; private set; } = null!;
    public string Apellidos { get; private set; } = null!;
    public CorreoElectronico Correo { get; private set; }
    public string Cargo { get; private set; } = null!;
    public string AreaOrganizacional { get; private set; } = null!;
    public EstadoLaboral Estado { get; private set; }
    public DateTime FechaCreacionUtc { get; private set; }

    private Empleado() { } // Entity Framework Core

    public static Empleado Registrar(string documento, string nombres, string apellidos,
        string correo, string cargo, string areaOrganizacional)
    {
        var empleado = new Empleado
        {
            Id = Guid.NewGuid(), Estado = EstadoLaboral.Activo, FechaCreacionUtc = DateTime.UtcNow
        };
        empleado.Actualizar(documento, nombres, apellidos, correo, cargo, areaOrganizacional);
        return empleado;
    }

    public void Actualizar(string documento, string nombres, string apellidos,
        string correo, string cargo, string areaOrganizacional)
    {
        Documento = TextoObligatorio.Validar(documento, "El documento", 30);
        Nombres = TextoObligatorio.Validar(nombres, "Los nombres", 100);
        Apellidos = TextoObligatorio.Validar(apellidos, "Los apellidos", 100);
        Correo = CorreoElectronico.Crear(correo);
        Cargo = TextoObligatorio.Validar(cargo, "El cargo", 120);
        AreaOrganizacional = TextoObligatorio.Validar(areaOrganizacional, "El área organizacional", 120);
    }

    public void Desactivar() => Estado = EstadoLaboral.Inactivo;
}
