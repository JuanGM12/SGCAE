namespace SGCAE.Dominio.Compartido;

public sealed class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}

public static class TextoObligatorio
{
    public static string Validar(string? valor, string campo, int maximo)
    {
        var limpio = valor?.Trim() ?? string.Empty;
        if (limpio.Length == 0) throw new ReglaNegocioException($"{campo} es obligatorio.");
        if (limpio.Length > maximo) throw new ReglaNegocioException($"{campo} no puede superar {maximo} caracteres.");
        return limpio;
    }
}
