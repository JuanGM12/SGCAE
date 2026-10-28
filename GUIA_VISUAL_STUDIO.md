# Guía detallada para abrir, ejecutar, probar y entregar SGCAE en Visual Studio

## A. ¿Debo crear un proyecto o importar un archivo?

**No debes crear un proyecto desde cero.** Descarga y descomprime el ZIP suministrado. Busca el archivo **`SGCAE.sln`** (solución completa); es el que debes abrir con Visual Studio. Las clases, paquetes, proyectos y referencias ya están organizados.

Tampoco debes importar `.sql`: el esquema se crea a través de la migración de Entity Framework Core incluida. Puedes usar `scripts/consultas-sqlserver.sql` solamente para consultar las tablas y tomar evidencias.

## B. Instalar las herramientas en Windows 11

1. Instala/abre **Visual Studio 2026 Community** desde https://visualstudio.microsoft.com/es/vs/community/ . En el instalador marca la carga **Desarrollo de ASP.NET y web**. Comprueba que está instalado el **SDK de .NET 10**. `net10.0` requiere Visual Studio 2026 18.0 o superior; Visual Studio 2022 no lo soporta directamente.
2. Instala SQL Server LocalDB (normalmente seleccionable en componentes individuales de Visual Studio) o **SQL Server 2022/2025 Express**. Para una entrega académica basta **LocalDB**. Puedes administrar las tablas con **SQL Server Object Explorer** de Visual Studio o con **SQL Server Management Studio (SSMS)**.
3. (Opcional para GitHub) Instala Git para Windows desde https://git-scm.com/downloads/win .
4. En PowerShell comprueba:

```powershell
dotnet --version
sqllocaldb info
```

El primer comando debe mostrar un SDK con número inicial `10.`. El segundo debería incluir `MSSQLLocalDB` si instalaste LocalDB. Si `sqllocaldb` no se reconoce, instala SQL Server Express LocalDB o cambia a tu servidor SQL real.

## C. Extraer el ZIP en una carpeta adecuada

1. Descarga `SGCAE_Proyecto_Completo.zip`.
2. Clic derecho > **Extraer todo**. Se recomienda `C:\Proyectos\SGCAE`.
3. Confirma que dentro de esa carpeta están `SGCAE.sln`, `src`, `tests`, `scripts` y `README.md`.
4. Si al extraer aparece `C:\Proyectos\SGCAE\SGCAE\SGCAE.sln`, abre esa carpeta interna, no la externa.

## D. Abrir la solución sin copiar archivos

1. Abre Visual Studio 2026.
2. En la pantalla inicial selecciona **Abrir un proyecto o una solución**.
3. Navega a `C:\Proyectos\SGCAE\SGCAE.sln` (ajusta a tu ruta real) y pulsa **Abrir**.
4. Abre **Ver > Explorador de soluciones** si no está visible (Ctrl+Alt+L).
5. Debes ver cinco proyectos: `SGCAE.Dominio`, `SGCAE.Aplicacion`, `SGCAE.Infraestructura`, `SGCAE.Api`, `SGCAE.Pruebas`.
6. En la solución, clic derecho > **Restaurar paquetes NuGet**. Espera a que terminen los paquetes en el panel **Salida**. Necesitas internet en esta primera restauración.
7. Menú **Compilar > Compilar solución** (Ctrl+Mayús+B). No debería haber errores de compilación. Si hay errores por paquetes, verifica SDK y haz de nuevo Restaurar.

## E. Configurar SQL Server

### Opción 1: LocalDB — más sencillo

1. Abre `src > SGCAE.Api > appsettings.json`.
2. Comprueba este bloque:

```json
"ConnectionStrings": {
  "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=SGCAE_Db;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

3. Abre PowerShell y ejecuta `sqllocaldb start MSSQLLocalDB` si hace falta.
4. Al ejecutar la API por primera vez, EF Core creará `SGCAE_Db` y las tablas.

### Opción 2: SQL Server Express

1. Verifica el nombre del servidor con SQL Server Management Studio (por ejemplo `localhost\\SQLEXPRESS`, o `localhost` en una instalación predeterminada).
2. Cambia el valor `SqlServer` a:

```json
"SqlServer": "Server=.\\SQLEXPRESS;Database=SGCAE_Db;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

3. Guardar (Ctrl+S), vuelve a iniciar la API. Ajusta el nombre de instancia si no es `SQLEXPRESS`. Activa el servicio de la instancia de SQL Server desde los Servicios de Windows si está detenido.

**Nota:** Las barras dobles (`\\`) son necesarias en el archivo JSON. El certificado se acepta solo para desarrollo local. No debes subir contraseñas a GitHub.

## F. Establecer la API como proyecto inicial y ejecutar

1. En Explorador de soluciones haz clic derecho a **SGCAE.Api** > **Establecer como proyecto de inicio**.
2. En la barra superior selecciona el perfil `http` (o usa el perfil `https` si tu certificado local funciona).
3. Presiona **F5** o el botón verde Iniciar. Si prefieres ejecutar sin depurador, **Ctrl+F5**.
4. En la ventana de consola deberían aparecer mensajes similares a `Now listening on: http://localhost:5124` y `Application started`.
5. Navega a **http://localhost:5124/swagger** si no abre automáticamente. Verás las secciones Accesos, Credenciales, Empleados y PuntosAcceso.
6. La API aplica la migración inicial durante el inicio en el entorno `Development`. Si ya existía la base y fue creada por otra vía, consulta la sección de problemas antes de mezclar esquemas.

## G. Probar TODOS los requisitos usando Swagger

En cada operación abre el desplegable, pulsa **Try it out**, llena JSON/parámetros, pulsa **Execute**, y revisa código HTTP y Response body.

### G1. RF-01: registrar y consultar empleado

En `POST /api/empleados` pega:

```json
{
  "documento": "1045123456",
  "nombres": "Andrea",
  "apellidos": "Gómez",
  "correo": "andrea.gomez@empresa.com",
  "cargo": "Auxiliar logística",
  "areaOrganizacional": "Almacenamiento"
}
```

Respuesta esperada **201 Created**. Guarda `id` (un UUID/GUID). Usa `GET /api/empleados/{id}` y `GET /api/empleados`. Prueba `PUT /api/empleados/{id}` con el mismo JSON cambiando `cargo` a `Supervisora`; después de la demo prueba `PATCH /api/empleados/{id}/desactivar`.

### G2. RF-06: crear punto de acceso

`POST /api/puntos-acceso`:

```json
{"codigo":"BODEGA","nombre":"Puerta Bodega","ubicacion":"Zona Industrial","habilitado":true}
```

Respuesta **201**. Usa `GET /api/puntos-acceso` para listar; guarda el `id`. Para deshabilitarlo usa `PATCH /api/puntos-acceso/{id}/estado` con `{"habilitado":false}`.

### G3. RF-02: asignar credencial

`POST /api/credenciales`, reemplaza el GUID por el `id` real del empleado:

```json
{"empleadoId":"GUID_DEL_EMPLEADO","codigo":"TARJETA-100"}
```

Respuesta **201** y `estado: "Vigente"`. Guarda su `id`. Verifica `GET /api/credenciales/{id}` y `GET /api/credenciales/empleado/{empleadoId}`. Prueba `PATCH /api/credenciales/{id}/bloquear` y `PATCH /api/credenciales/{id}/revocar` **después** de probar entrada/salida autorizadas.

### G4. RF-03: entrada y salida

`POST /api/accesos`:

```json
{"codigoCredencial":"TARJETA-100","codigoPuntoAcceso":"BODEGA","tipo":"Entrada"}
```

Esperado: HTTP **200** y `resultado: "Autorizado"`. Repite exactamente la entrada y verás `resultado: "Rechazado"` con motivo de movimiento duplicado. Registra `"Salida"` y verás `"Autorizado"` otra vez. Bloquea la credencial y vuelve a intentar `"Entrada"` → `"Rechazado"` con motivo de bloqueo. También prueba una tarjeta inexistente: `"codigoCredencial":"NO-EXISTE"` → rechazado y registrado.

**Importante:** los intentos rechazados se guardan con HTTP 200 para indicar que el intento fue auditado. La autorización física se decide por el campo `resultado`.

### G5. RF-04: historial con filtros y paginación

`GET /api/accesos/historial` con `pagina=1` y `tamanoPagina=2`: comprobarás solo dos registros por página. Repite con `pagina=2`. Luego combina `resultado=Rechazado`, `empleadoId=GUID`, `codigoPuntoAcceso=BODEGA` o rango de fechas (`2026-10-01T00:00:00Z` a `2026-11-01T23:59:59Z`).

## H. Prueba automática real desde PowerShell

Con la API abierta en `http://localhost:5124`, en Visual Studio selecciona **Ver > Terminal** (o abre PowerShell) y ve a la carpeta del proyecto:

```powershell
cd C:\Proyectos\SGCAE
powershell -ExecutionPolicy Bypass -File .\scripts\Demo-SGCAE.ps1
```

El script crea datos **únicos** y comprueba 10 situaciones (incluidos rechazos, paginación, bloqueo, baja del empleado); si todo es correcto, imprime `LISTO: DEMOSTRACION TERMINADA CORRECTAMENTE`. Si usas un puerto distinto:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Demo-SGCAE.ps1 -Url "http://localhost:OTRO_PUERTO"
```

## I. Ejecutar pruebas xUnit

1. Menú **Prueba > Explorador de pruebas**.
2. Selecciona **Ejecutar todas**.
3. Se deben ejecutar pruebas sobre validaciones de dominio y manejadores sin necesitar SQL Server. Puedes ejecutar también `dotnet test SGCAE.sln` desde la terminal.
4. Captura el resultado una vez haya terminado y verifiques que aparezca correctamente (no presupongas que pasó sin ejecutarlo).

## J. Comprobar tablas en SQL Server

1. Menú **Ver > Explorador de objetos de SQL Server**; selecciona **Agregar SQL Server** o Conectar.
2. Nombre del servidor: `(localdb)\MSSQLLocalDB` (o `.\SQLEXPRESS`). Autenticación de Windows.
3. Expande `SQL Server > (localdb)\MSSQLLocalDB > Databases > SGCAE_Db > Tables`.
4. Busca las cuatro tablas: `dbo.Empleados`, `dbo.Credenciales`, `dbo.PuntosAcceso`, `dbo.MovimientosAcceso`. Verifica también `dbo.__EFMigrationsHistory`.
5. Clic derecho sobre una tabla > **Ver datos** para observar datos creados por Swagger o el script de demo. O ejecuta `scripts/consultas-sqlserver.sql` en SSMS.

## K. Subir a GitHub (individual)

1. Entra a https://github.com/new e inicia sesión.
2. Nombre: **SGCAE**. Visibilidad: **Public**. No marques README, `.gitignore` ni licencia porque el ZIP ya los contiene. Pulsa **Create repository**.
3. Desde una terminal en `C:\Proyectos\SGCAE`:

```powershell
git init
git add .
git commit -m "feat: sistema control de acceso de empleados en .NET"
git branch -M main
git remote add origin https://github.com/TU_USUARIO/SGCAE.git
git push -u origin main
```

4. Sustituye `TU_USUARIO` por el nombre de tu cuenta de GitHub. Si Git pide autenticación usa tu navegador/Git Credential Manager. Si Git te pide nombre y correo, ejecuta `git config --global user.name "Tu Nombre"` y `git config --global user.email "tu-correo@example.com"` y repite el commit.
5. Abre en el navegador tu repositorio. Entra a la pestaña **Actions**: el workflow compila y ejecuta los tests en GitHub. Revisa que pase; si falla, inspecciona los logs y corrige.
6. Comparte con el profesor el **link público** `https://github.com/TU_USUARIO/SGCAE` (no un ZIP privado ni el archivo `.sln` suelto).

## L. Evidencias recomendadas para sustentar

- [ ] Explorador de soluciones mostrando las cuatro capas y tests.
- [ ] Swagger mostrando los endpoints y respuesta 201 al crear empleado.
- [ ] Swagger mostrando `resultado: "Autorizado"` en una entrada.
- [ ] Swagger mostrando `resultado: "Rechazado"` en empleado inactivo/credencial bloqueada.
- [ ] Consulta `historial` con paginación y filtro por resultado.
- [ ] SQL Server con tablas y datos de movimientos.
- [ ] Explorador de pruebas en verde.
- [ ] Repositorio de GitHub **público** y, si se utiliza, Actions en verde.

## M. Problemas frecuentes

**NETSDK1045 o versión .NET no soportada:** has abierto con Visual Studio 2022 o SDK anterior. Instala **Visual Studio 2026 + .NET 10 SDK**. No edites solo `net10.0` a `net8.0`: también habría que cambiar los paquetes EF Core.

**Cannot open database / login failed / LocalDB instance no encontrada:** comprueba que LocalDB esté instalada con `sqllocaldb info`; inicia con `sqllocaldb start MSSQLLocalDB`; verifica la cadena. Usa autenticación Windows y permisos locales.

**Certificate chain was issued by an authority that is not trusted:** en desarrollo local comprueba `TrustServerCertificate=True` en cadena; para entornos reales utiliza un certificado confiable.

**The TCP port 5124 is already in use:** cierra la instancia anterior o cambia `applicationUrl` en `src/SGCAE.Api/Properties/launchSettings.json`. Ajusta el URL usado en pruebas PowerShell.

**Swagger no abre:** revisa que el proyecto inicial sea SGCAE.Api, perfil `http`, ambiente `Development`, y consola sin errores. Abre manualmente `http://localhost:5124/swagger`.

**NuGet no restaura:** comprueba conexión a internet, SDK y origen `nuget.org` en **Herramientas > Administrador de paquetes NuGet > Configuración**.

**Already an object named Empleados (base creada manualmente):** no mezcles `EnsureCreated`/scripts manuales con migraciones en una BD que ya tiene tablas. Para esta práctica, si NO necesitas datos de esa BD, elimina solo la base académica `SGCAE_Db` y deja que la API la cree de nuevo (¡no borres una BD con datos que necesites!).

**GitHub dice repository not found / authentication failed:** verifica que creaste el repositorio público vacío y la URL remota, y haz login en navegador.
