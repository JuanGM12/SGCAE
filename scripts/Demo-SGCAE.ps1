# Ejecutar con API iniciada: powershell -ExecutionPolicy Bypass -File .\scripts\Demo-SGCAE.ps1
param([string]$Url = "http://localhost:5124")
$ErrorActionPreference = "Stop"
$stamp = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
$documento = "DOC-$stamp"
$tarjeta = "TAR-$stamp"
$puntoCodigo = "PT-$stamp"
function Enviar($ruta, $datos, $metodo="POST") {
    return Invoke-RestMethod -Method $metodo -Uri "$Url$ruta" -ContentType 'application/json' -Body ($datos | ConvertTo-Json -Depth 5)
}
function Exigir($condicion, $mensaje) { if (-not $condicion) { throw "ERROR: $mensaje" } }
Write-Host "========== DEMO SGCAE ========== " -ForegroundColor Cyan
Write-Host "1. Registro de empleado"
$empleado = Enviar '/api/empleados' @{
    documento=$documento;nombres='Laura';apellidos='Garcia';correo='laura@ejemplo.com';cargo='Analista';areaOrganizacional='Bodega'
}
Exigir ($empleado.estado -eq 'Activo') 'Empleado no esta activo'
Write-Host "OK empleado: $($empleado.id)" -ForegroundColor Green
Write-Host '2. Registro del punto de acceso'
$punto = Enviar '/api/puntos-acceso' @{codigo=$puntoCodigo;nombre='Bodega principal';ubicacion='Zona A';habilitado=$true}
Exigir ($punto.habilitado) 'Punto no habilitado'
Write-Host "OK punto: $($punto.id)" -ForegroundColor Green
Write-Host '3. Asignacion de credencial'
$credencial = Enviar '/api/credenciales' @{empleadoId=$empleado.id;codigo=$tarjeta}
Exigir ($credencial.estado -eq 'Vigente') 'Credencial no vigente'
Write-Host "OK credencial: $($credencial.id)" -ForegroundColor Green
Write-Host '4. Entrada autorizada'
$entrada = Enviar '/api/accesos' @{codigoCredencial=$tarjeta;codigoPuntoAcceso=$puntoCodigo;tipo='Entrada'}
Exigir ($entrada.resultado -eq 'Autorizado') 'Entrada debe autorizarse'
Write-Host "OK: $($entrada.resultado)" -ForegroundColor Green
Write-Host '5. Entrada consecutiva rechazada y auditada'
$duplicado = Enviar '/api/accesos' @{codigoCredencial=$tarjeta;codigoPuntoAcceso=$puntoCodigo;tipo='Entrada'}
Exigir ($duplicado.resultado -eq 'Rechazado') 'Duplicado debe rechazarse'
Write-Host "OK: $($duplicado.motivo)" -ForegroundColor Green
Write-Host '6. Salida autorizada'
$salida = Enviar '/api/accesos' @{codigoCredencial=$tarjeta;codigoPuntoAcceso=$puntoCodigo;tipo='Salida'}
Exigir ($salida.resultado -eq 'Autorizado') 'Salida debe autorizarse'
Write-Host "OK: $($salida.resultado)" -ForegroundColor Green
Write-Host '7. Bloqueo de credencial'
$bloqueada = Invoke-RestMethod -Method PATCH -Uri "$Url/api/credenciales/$($credencial.id)/bloquear"
Exigir ($bloqueada.estado -eq 'Bloqueada') 'Bloqueo no aplicado'
$rechazado = Enviar '/api/accesos' @{codigoCredencial=$tarjeta;codigoPuntoAcceso=$puntoCodigo;tipo='Entrada'}
Exigir ($rechazado.resultado -eq 'Rechazado') 'Credencial bloqueada debe rechazarse'
Write-Host "OK: $($rechazado.motivo)" -ForegroundColor Green
Write-Host '8. Historial paginado'
$historial = Invoke-RestMethod -Uri "$Url/api/accesos/historial?empleadoId=$($empleado.id)&pagina=1&tamanoPagina=2"
Exigir ($historial.total -eq 4) 'Se esperaban 4 movimientos guardados'
Exigir ($historial.elementos.Count -eq 2) 'Paginacion incorrecta'
Write-Host "OK: $($historial.total) registros; $($historial.elementos.Count) por pagina" -ForegroundColor Green
Write-Host '9. Desactivar empleado y comprobar rechazo'
$desactivado = Invoke-RestMethod -Method PATCH -Uri "$Url/api/empleados/$($empleado.id)/desactivar"
Exigir ($desactivado.estado -eq 'Inactivo') 'No se desactivo'
$rechazoInactivo = Enviar '/api/accesos' @{codigoCredencial=$tarjeta;codigoPuntoAcceso=$puntoCodigo;tipo='Entrada'}
Exigir ($rechazoInactivo.resultado -eq 'Rechazado') 'Empleado inactivo debe rechazarse'
Write-Host "OK: $($rechazoInactivo.motivo)" -ForegroundColor Green
Write-Host '10. Consultar solo rechazos'
$rechazos = Invoke-RestMethod -Uri "$Url/api/accesos/historial?resultado=Rechazado&empleadoId=$($empleado.id)&pagina=1&tamanoPagina=20"
Exigir ($rechazos.total -eq 3) 'Deben existir 3 rechazos'
Write-Host "OK: $($rechazos.total) rechazos" -ForegroundColor Green
Write-Host "LISTO: DEMOSTRACION TERMINADA CORRECTAMENTE." -ForegroundColor Cyan
