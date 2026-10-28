using Microsoft.EntityFrameworkCore;
using SGCAE.Aplicacion.Abstracciones;
using SGCAE.Aplicacion.Accesos;
using SGCAE.Aplicacion.Credenciales;
using SGCAE.Aplicacion.Empleados;
using SGCAE.Aplicacion.PuntosAcceso;
using SGCAE.Infraestructura.Persistencia;
using SGCAE.Infraestructura.Repositorios;

namespace SGCAE.Api.Configuracion;

public static class ServiciosProyecto
{
    public static IServiceCollection AgregarSgcae(this IServiceCollection services, IConfiguration configuration)
    {
        var conexion = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:SqlServer en la configuración.");
        services.AddDbContext<SgcaeDbContext>(opt => opt.UseSqlServer(conexion));
        services.AddScoped<IUnidadTrabajo>(sp => sp.GetRequiredService<SgcaeDbContext>());
        services.AddScoped<IRepositorioEmpleados, RepositorioEmpleados>();
        services.AddScoped<IRepositorioCredenciales, RepositorioCredenciales>();
        services.AddScoped<IRepositorioPuntosAcceso, RepositorioPuntosAcceso>();
        services.AddScoped<IRepositorioMovimientos, RepositorioMovimientos>();

        // Registro explícito CQRS: cada comando y consulta tiene su propio manejador.
        services.AddScoped<IComandoManejador<CrearEmpleadoComando, EmpleadoDto>, CrearEmpleadoManejador>();
        services.AddScoped<IComandoManejador<ActualizarEmpleadoComando, EmpleadoDto>, ActualizarEmpleadoManejador>();
        services.AddScoped<IComandoManejador<DesactivarEmpleadoComando, EmpleadoDto>, DesactivarEmpleadoManejador>();
        services.AddScoped<IConsultaManejador<ObtenerEmpleadoConsulta, EmpleadoDto>, ObtenerEmpleadoManejador>();
        services.AddScoped<IConsultaManejador<ListarEmpleadosConsulta, Pagina<EmpleadoDto>>, ListarEmpleadosManejador>();

        services.AddScoped<IComandoManejador<AsignarCredencialComando, CredencialDto>, AsignarCredencialManejador>();
        services.AddScoped<IComandoManejador<CambiarEstadoCredencialComando, CredencialDto>, CambiarEstadoCredencialManejador>();
        services.AddScoped<IConsultaManejador<ObtenerCredencialConsulta, CredencialDto>, ObtenerCredencialManejador>();
        services.AddScoped<IConsultaManejador<ListarCredencialesEmpleadoConsulta, IReadOnlyList<CredencialDto>>, ListarCredencialesEmpleadoManejador>();

        services.AddScoped<IComandoManejador<RegistrarPuntoAccesoComando, PuntoAccesoDto>, RegistrarPuntoAccesoManejador>();
        services.AddScoped<IComandoManejador<CambiarEstadoPuntoAccesoComando, PuntoAccesoDto>, CambiarEstadoPuntoAccesoManejador>();
        services.AddScoped<IConsultaManejador<ObtenerPuntoAccesoConsulta, PuntoAccesoDto>, ObtenerPuntoAccesoManejador>();
        services.AddScoped<IConsultaManejador<ListarPuntosAccesoConsulta, IReadOnlyList<PuntoAccesoDto>>, ListarPuntosAccesoManejador>();

        services.AddScoped<IComandoManejador<RegistrarAccesoComando, MovimientoAccesoDto>, RegistrarAccesoManejador>();
        services.AddScoped<IConsultaManejador<ConsultarHistorialConsulta, Pagina<MovimientoAccesoDto>>, ConsultarHistorialManejador>();
        return services;
    }
}
