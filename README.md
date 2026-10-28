# SGCAE — Sistema de Gestión y Control de Acceso de Empleados

Proyecto académico individual basado en **Seguimiento 2**. Backend REST desarrollado en **ASP.NET Core Web API (.NET 10 / C#)** con **SQL Server**, **Entity Framework Core 10**, **Clean Architecture**, **DDD** y **CQRS**. Incluye solución Visual Studio, migración inicial, Swagger, pruebas xUnit, demostración PowerShell, consultas SQL y CI en GitHub Actions.

> IMPORTANTE: es un backend, no una interfaz web de login. Swagger permite demostrar los endpoints. No se requiere importar scripts para construir tablas: EF Core aplica la migración automáticamente al iniciar en `Development`.

## Requisitos del equipo

- Windows 10/11, Visual Studio **2026 (18.0+)** con carga **Desarrollo de ASP.NET y web** y SDK .NET 10. Si utilizas VS 2022, este proyecto `net10.0` requiere actualizar Visual Studio o usar `dotnet` por terminal.
- SQL Server **LocalDB**, SQL Server Express o SQL Server completo, con autorización para crear la base.
- Acceso a internet al restaurar los paquetes NuGet por primera vez.
- Git instalado si vas a subir el repositorio público.

## Inicio rápido con Visual Studio

1. Descomprime el ZIP y entra a la carpeta **SGCAE** (la que contiene `SGCAE.sln`).
2. En Visual Studio 2026 selecciona **Abrir un proyecto o una solución** y abre **`SGCAE.sln`**. **No** crees otro proyecto ni copies las clases a mano.
3. Visual Studio restaurará paquetes NuGet. Si no lo hace, clic derecho sobre la solución > **Restaurar paquetes NuGet**.
4. Verifica el servidor SQL en `src/SGCAE.Api/appsettings.json`. Por defecto se usa `(localdb)\\MSSQLLocalDB`, una instancia local habitual de Visual Studio.
5. En el Explorador de soluciones, clic derecho sobre **SGCAE.Api** > **Establecer como proyecto de inicio**.
6. Arriba selecciona el perfil **http** y presiona **F5** / **Ctrl+F5**.
7. Se abre `http://localhost:5124/swagger`. La primera ejecución crea `SGCAE_Db` y sus tablas con EF Core Migrations.
8. En Swagger expande **POST /api/empleados** > **Try it out** > **Execute**. Luego crea punto, credencial, acceso y consulta historial.
9. Para pruebas completas automáticas, abre una terminal PowerShell en `SGCAE` y ejecuta:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Demo-SGCAE.ps1
```

10. En **Explorador > SQL Server Object Explorer** (o SSMS), conéctate a `(localdb)\MSSQLLocalDB` y revisa `Databases > SGCAE_Db > Tables`.

Lee [GUIA_VISUAL_STUDIO.md](GUIA_VISUAL_STUDIO.md) para instrucciones detalladas, errores frecuentes y GitHub.

## Estructura y dependencias

```text
SGCAE.sln
src/
  SGCAE.Dominio/          - entidades + objetos de valor + invariantes (sin dependencias)
  SGCAE.Aplicacion/       - comandos / consultas / handlers / interfaces / DTO
  SGCAE.Infraestructura/  - EF Core, SQL Server, repositorios, migracion
  SGCAE.Api/              - ASP.NET Core, controladores, middleware, Swagger
  
tests/
  SGCAE.Pruebas/          - pruebas xUnit de reglas y casos de uso
scripts/
  Demo-SGCAE.ps1          - demo end-to-end que usa una API ejecutándose
  consultas-sqlserver.sql - verificación de persistencia
SGCAE.http                - solicitudes HTTP individuales para IDE
.github/workflows/        - compilación y pruebas en GitHub
```

Principio de dependencias: **Api → Aplicacion, Infraestructura; Infraestructura → Aplicacion → Dominio**. El dominio no conoce EF, HTTP ni SQL. La API no implementa reglas de negocio. CQRS separa comandos que escriben de consultas que leen (cada uno con manejador), usando una sola base de datos porque el ejercicio no requiere doble persistencia.

## Endpoints

| Método | Endpoint | RF | Acción |
|---|---|---|---|
| POST | `/api/empleados` | RF-01 | Registrar empleado |
| GET | `/api/empleados` | RF-01 | Listar con `pagina`, `tamanoPagina` |
| GET | `/api/empleados/{id}` | RF-01 | Consultar uno |
| PUT | `/api/empleados/{id}` | RF-01 | Actualizar |
| PATCH | `/api/empleados/{id}/desactivar` | RF-01 | Desactivar sin borrar |
| POST | `/api/credenciales` | RF-02 | Asignar a empleado activo |
| GET | `/api/credenciales/{id}` | RF-02 | Consultar estado |
| GET | `/api/credenciales/empleado/{empleadoId}` | RF-02 | Consultar credenciales del empleado |
| PATCH | `/api/credenciales/{id}/bloquear` | RF-02 | Bloquear |
| PATCH | `/api/credenciales/{id}/revocar` | RF-02 | Revocar definitivamente |
| POST | `/api/accesos` | RF-03 | Registrar Entrada / Salida, incluso Rechazado |
| GET | `/api/accesos/historial` | RF-04 | Filtrar/paginar historial |
| POST | `/api/puntos-acceso` | RF-06 | Registrar punto físico |
| GET | `/api/puntos-acceso` | RF-06 | Listar puntos |
| GET | `/api/puntos-acceso/{id}` | RF-06 | Consultar uno |
| PATCH | `/api/puntos-acceso/{id}/estado` | RF-06 | Habilitar/deshabilitar |

Nota: el documento original enumera RF-01, RF-02, RF-03, RF-04 y RF-06; no define RF-05.

## Ejemplo: flujo de prueba por Swagger

### 1. `POST /api/empleados`

```json
{
  "documento": "1000123456",
  "nombres": "Laura",
  "apellidos": "García",
  "correo": "laura@empresa.com",
  "cargo": "Auxiliar de logística",
  "areaOrganizacional": "Bodega"
}
```

Guarda el campo **id** de la respuesta (HTTP 201). El estado inicial es **Activo**. `documento` es único.

### 2. `POST /api/puntos-acceso`

```json
{
  "codigo": "BODEGA-01",
  "nombre": "Puerta principal bodega",
  "ubicacion": "Bodega central, acceso norte",
  "habilitado": true
}
```

Guarda el **id** del punto. `codigo` es único y se normaliza a mayúsculas.

### 3. `POST /api/credenciales`

```json
{
  "empleadoId": "REEMPLAZA_POR_EL_GUID_DEL_EMPLEADO",
  "codigo": "TAR-0001"
}
```

Guarda el **id** de la credencial. Solo puede asignarse a un empleado Activo. No se permiten dos credenciales Vigentes al mismo empleado ni repetir el código.

### 4. `POST /api/accesos` (autorización)

```json
{
  "codigoCredencial": "TAR-0001",
  "codigoPuntoAcceso": "BODEGA-01",
  "tipo": "Entrada"
}
```

HTTP **200**, `resultado: "Autorizado"` si cumple todas las reglas. Repite la misma entrada: HTTP **200**, `resultado: "Rechazado"`, con motivo de duplicación. **Se guarda también el rechazo**. Registra después una `"Salida"`: autorizada.

> Un rechazo normal de acceso se responde con HTTP 200 porque el procesamiento/auditoría del intento fue exitoso, aunque el ingreso físico no fue autorizado. La aplicación cliente siempre debe consultar el campo `resultado`; no debe considerar HTTP 200 como autorización física.

### 5. `GET /api/accesos/historial`

```text
/api/accesos/historial?pagina=1&tamanoPagina=2
/api/accesos/historial?empleadoId=GUID&resultado=Rechazado&pagina=1&tamanoPagina=10
/api/accesos/historial?codigoPuntoAcceso=BODEGA-01&desdeUtc=2026-10-01T00:00:00Z&hastaUtc=2026-11-01T23:59:59Z
```

Filtros disponibles: `empleadoId` (GUID), `desdeUtc`, `hastaUtc` (fechas UTC), `codigoPuntoAcceso`, `resultado` (`Autorizado` o `Rechazado`), `pagina` y `tamanoPagina` (máximo 100). La respuesta devuelve `elementos`, `total`, `paginaActual`, `tamanoPagina`, `totalPaginas`.

### 6. Probar bloqueos y bajas

- `PATCH /api/credenciales/{id}/bloquear` → siguiente acceso es Rechazado.
- `PATCH /api/credenciales/{id}/revocar` → siguiente acceso es Rechazado; no se puede bloquear una revocada.
- `PATCH /api/empleados/{id}/desactivar` → siguiente acceso es Rechazado, incluso con credencial vigente.
- `PATCH /api/puntos-acceso/{id}/estado` con `{ "habilitado": false }` → siguiente acceso es Rechazado.

## Persistencia y migraciones

La migración `202610280001_Inicial` crea `Empleados`, `Credenciales`, `PuntosAcceso`, `MovimientosAcceso`, índices únicos, índices de historial y relaciones con llaves foráneas. Se aplica automáticamente cuando la API se inicia en Development. La BD se llama `SGCAE_Db` en el connection string predeterminado. Para ver la tabla de migraciones se usa `__EFMigrationsHistory`.

Si usas SQL Server Express, cambia solamente la cadena a:

```json
"SqlServer": "Server=.\\SQLEXPRESS;Database=SGCAE_Db;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

La opción `TrustServerCertificate=True` se incluye para desarrollo local; en producción se debe validar el certificado del servidor correctamente. Si usas autenticación SQL, evita guardar usuario y contraseña reales en un repositorio público: configura **User Secrets**, variables de entorno o `appsettings.Local.json` fuera del repositorio.

## Compilar y ejecutar desde terminal (alternativa)

```powershell
dotnet --info
dotnet restore SGCAE.sln
dotnet build SGCAE.sln
dotnet test SGCAE.sln
dotnet run --project src/SGCAE.Api --launch-profile http
```

La ejecución por CLI usa el perfil `http`, que define `ASPNETCORE_ENVIRONMENT=Development`, por lo que ejecuta la migración inicial y habilita Swagger. La API escucha en `http://localhost:5124`.

## Control de calidad / estados HTTP

- `201 Created`: empleado, credencial o punto registrado.
- `200 OK`: consultas, cambios y solicitudes de acceso procesadas (incluido resultado `Rechazado`).
- `400 Bad Request`: argumentos estructuralmente inválidos, fechas/paginación inválidas, JSON malformado.
- `404 Not Found`: recurso por GUID inexistente.
- `409 Conflict`: identidad duplicada, código duplicado, regla de dominio incumplida.
- `500`: error inesperado registrado en logs sin devolver información sensible.

`tests/SGCAE.Pruebas` incluye pruebas de dominio y de aplicación usando repositorios falsos; no se necesita SQL Server para `dotnet test`. `scripts/Demo-SGCAE.ps1` hace una prueba funcional REST real y **sí** requiere SQL Server y la API iniciada.

## GitHub (entregable)

```powershell
git init
git add .
git commit -m "feat: SGCAE API con Clean Architecture DDD CQRS y SQL Server"
git branch -M main
git remote add origin https://github.com/TU_USUARIO/SGCAE.git
git push -u origin main
```

Primero crea un repositorio **público y vacío** llamado `SGCAE` en GitHub (sin README ni `.gitignore` iniciales). Cambia `TU_USUARIO` por tu usuario. El enlace final que debes entregar será `https://github.com/TU_USUARIO/SGCAE`.

## Advertencias técnicas

- Proyecto individual: el trabajo demuestra uso de repositorio Git, no requiere compañeros.
- Sin autenticación ni lectores físicos porque no aparecen en el requerimiento; para un producto real sería indispensable agregar autenticación, autorización, controles de concurrencia y políticas de seguridad.
- El entorno donde se generó el código no dispone del SDK .NET ni de SQL Server: las pruebas automatizadas se **incluyen** pero deben ejecutarse en el equipo del estudiante o en GitHub Actions para confirmar la compilación y conexión real.
