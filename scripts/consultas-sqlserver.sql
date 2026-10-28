-- Ejecutar en SQL Server Management Studio conectado a (localdb)\MSSQLLocalDB o tu servidor.
USE SGCAE_Db;
GO
-- RF-01: empleados y estado laboral (1 Activo, 2 Inactivo)
SELECT Id, Documento, Nombres, Apellidos, Correo, Cargo, AreaOrganizacional, Estado
FROM dbo.Empleados ORDER BY FechaCreacionUtc DESC;

-- RF-02: credenciales (1 Vigente, 2 Bloqueada, 3 Revocada)
SELECT c.Codigo, c.Estado, e.Documento, e.Nombres, e.Apellidos
FROM dbo.Credenciales AS c
JOIN dbo.Empleados AS e ON e.Id = c.EmpleadoId;

-- RF-06: puntos de acceso
SELECT Codigo, Nombre, Ubicacion, Habilitado FROM dbo.PuntosAcceso;

-- RF-03 y RF-04: historial (1 Entrada, 2 Salida; 1 Autorizado, 2 Rechazado)
SELECT TOP (100) FechaUtc, CodigoCredencial, CodigoPuntoAcceso, Tipo, Resultado,
  Motivo, EmpleadoId FROM dbo.MovimientosAcceso ORDER BY FechaUtc DESC;

-- Evidencia de rechazos
SELECT CodigoCredencial, CodigoPuntoAcceso, Motivo, FechaUtc
FROM dbo.MovimientosAcceso WHERE Resultado = 2 ORDER BY FechaUtc DESC;

-- Evidencia de EF Core Migrations
SELECT * FROM dbo.__EFMigrationsHistory;
