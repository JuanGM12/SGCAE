using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SGCAE.Api.Configuracion;
using SGCAE.Api.Middleware;
using SGCAE.Infraestructura.Persistencia;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(opciones =>
    opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AgregarSgcae(builder.Configuration);

var app = builder.Build();
app.UseMiddleware<ControlErroresMiddleware>();

if (app.Environment.IsDevelopment())
{
    // Crea/actualiza el esquema usando EF Core Migrations al iniciar en desarrollo.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SgcaeDbContext>();
    await db.Database.MigrateAsync();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();

// Facilita futuros tests de integración con WebApplicationFactory.
public partial class Program { }
