# SGCAE — Sistema de Gestión y Control de Acceso de Empleados

Desarrollador: Juan Diego Patiño Osorio

La aplicación expone una API REST para registrar empleados, asignar/bloquear/revocar credenciales, administrar puntos de acceso y registrar entradas, salidas y accesos rechazados. La documentación y prueba interactiva se hace con **Swagger**.


## Ejecutar con Visual Studio

1. Descarga/clona este repositorio y abre `SGCAE.sln`
2. En el Explorador de soluciones, clic derecho en la solución > **Restaurar paquetes NuGet**.
3. Abre `src/SGCAE.Api/appsettings.json` y configura `ConnectionStrings:SqlServer` para tu SQL Server. Ejemplo para servidor local y autenticación de Windows:

   ```json
   "ConnectionStrings": {
     "SqlServer": "Server=localhost;Database=SGCAE_Db;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
   }
   ```

   Para **SQL Server Express** usa `Server=.\\SQLEXPRESS`. Para **LocalDB** usa `Server=(localdb)\\MSSQLLocalDB` (ambos dentro de un valor JSON, con doble barra invertida).
4. Clic derecho en **SGCAE.Api** > **Establecer como proyecto de inicio**.
5. En la barra superior, elige el perfil **http** y presiona **F5**.
6. Abre **http://localhost:5124/swagger**. En entorno `Development`, la API ejecuta las migraciones al iniciar y crea/actualiza `SGCAE_Db` si el usuario de SQL Server tiene permisos.

## Prueba rápida en Swagger

En cada endpoint pulsa **Try it out > Execute**. Para los identificadores `id`, copia los GUID reales de las respuestas.

### 1. Crear empleado (RF-01)

`POST /api/empleados`

```json
{"documento":"1000123456","nombres":"Laura","apellidos":"Garcia","correo":"laura@empresa.com","cargo":"Auxiliar de logistica","areaOrganizacional":"Bodega"}
```

Respuesta esperada: `201 Created`, con el `id` del empleado y estado `Activo`.

### 2. Crear punto de acceso (RF-06)

`POST /api/puntos-acceso`

```json
{"codigo":"BODEGA-01","nombre":"Puerta principal bodega","ubicacion":"Zona norte","habilitado":true}
```

Respuesta esperada: `201 Created`.

### 3. Asignar credencial (RF-02)

`POST /api/credenciales` (reemplaza el GUID con el `id` del empleado):

```json
{"empleadoId":"00000000-0000-0000-0000-000000000000","codigo":"TAR-0001"}
```

Respuesta esperada: `201 Created`, estado `Vigente` y `id` de la credencial.

### 4. Registrar entrada, entrada duplicada y salida (RF-03)

`POST /api/accesos`:

```json
{"codigoCredencial":"TAR-0001","codigoPuntoAcceso":"BODEGA-01","tipo":"Entrada"}
```

Primera solicitud: `resultado = Autorizado`. Ejecuta **otra vez** la misma solicitud: `resultado = Rechazado`, registrado en la auditoría.

Para una salida, cambia `"tipo":"Salida"`: resultado esperado `Autorizado`.

> Un intento rechazado puede responder con **HTTP 200**: significa que el intento se procesó y quedó registrado, **no** que la entrada haya sido autorizada. Revisa siempre `resultado`.

### 5. Bloquear y verificar rechazo (RF-02 y RF-03)

Ejecuta `PATCH /api/credenciales/{id}/bloquear` con el GUID de la credencial. Repite `POST /api/accesos` con `"tipo":"Entrada"`: debería registrar `Rechazado`.

### 6. Historial filtrado y paginado (RF-04)

`GET /api/accesos/historial?pagina=1&tamanoPagina=2&codigoPuntoAcceso=BODEGA-01`

Filtros: `empleadoId`, `desdeUtc`, `hastaUtc`, `codigoPuntoAcceso`, `resultado`, `pagina` y `tamanoPagina`. Confirma `elementos`, `total` y la paginación.

### 7. Más funciones

- `GET /api/empleados` y `GET /api/empleados/{id}` para consultas.
- `PUT /api/empleados/{id}` para actualización (mismos datos que crear).
- `PATCH /api/empleados/{id}/desactivar` para desactivar y conservar historial.
- `GET /api/credenciales/{id}` y `GET /api/credenciales/empleado/{empleadoId}`.
- `PATCH /api/credenciales/{id}/revocar` para revocación definitiva.
- `GET /api/puntos-acceso` y `GET /api/puntos-acceso/{id}`.
- `PATCH /api/puntos-acceso/{id}/estado` con `{"habilitado":false}` para deshabilitar.

## Verificar datos en SQL Server

En **SQL Server Management Studio (SSMS)**, conéctate a tu servidor y ejecuta:

```sql
USE SGCAE_Db;
SELECT MigrationId FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;
SELECT TOP (20) * FROM dbo.Empleados;
SELECT TOP (20) * FROM dbo.Credenciales;
SELECT TOP (20) * FROM dbo.PuntosAcceso;
SELECT TOP (20) * FROM dbo.MovimientosAcceso ORDER BY FechaUtc DESC;
```

## Estructura de la solución

```text
SGCAE.sln
src/
  SGCAE.Dominio/          Entidades, objetos de valor, reglas e invariantes DDD
  SGCAE.Aplicacion/       Comandos y consultas separados (CQRS), DTO y casos de uso
  SGCAE.Infraestructura/  DbContext, repositorios EF Core, migraciones SQL Server
  SGCAE.Api/              Controladores REST, DI, middleware y Swagger
tests/SGCAE.Pruebas/      Pruebas automatizadas xUnit
scripts/                  Demostración PowerShell y consultas SQL
Evidencias/               Capturas y documento de evidencias del seguimiento
```

**Arquitectura limpia:** el Dominio es independiente de SQL Server y ASP.NET Core; las dependencias apuntan hacia las capas internas. **CQRS:** comandos de modificación y consultas de lectura tienen manejadores separados.

## Evidencias del seguimiento

Las capturas de pantalla y el documento de evidencias de la entrega se encuentran en la carpeta **Evidencias**, ubicada en la **raíz del repositorio**, al mismo nivel que `SGCAE.sln`, `README.md`, `src/` y `tests/`.

La carpeta contiene las capturas de las pruebas realizadas en Swagger junto con Clean Architecture, DDD y CQRS.