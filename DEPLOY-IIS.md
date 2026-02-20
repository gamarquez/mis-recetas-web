# Guía de deploy en IIS — MisRecetas.Web

## Requisitos previos en el servidor

### 1. Instalar el ASP.NET Core Hosting Bundle

Este es el paso más importante. Descargarlo desde:

> **https://dotnet.microsoft.com/en-us/download/dotnet/9.0**
> → sección "ASP.NET Core Runtime" → **"Hosting Bundle"** (incluye Runtime + IIS Module)

Instalarlo y luego **reiniciar IIS**:

```
iisreset
```

Verificar que el módulo quedó instalado:

```powershell
Get-WebConfiguration -Filter system.webServer/globalModules -PSPath "IIS:\" |
  Select-Object -ExpandProperty Collection |
  Where-Object { $_.Name -like "*AspNetCore*" }
```

### 2. IIS habilitado con los roles necesarios

En **Agregar roles y características** asegurarse de tener:

- ✅ Servidor Web (IIS)
- ✅ Características HTTP comunes (Documento predeterminado, Errores HTTP, Contenido estático)
- ✅ Desarrollo de aplicaciones → **CGI** (requerido por el módulo ASP.NET Core)
- ✅ Herramientas de administración → Consola de administración de IIS

---

## Paso 1 — Publicar la aplicación

Desde la máquina de desarrollo, en la carpeta del proyecto:

```bash
dotnet publish MisRecetas.Web/MisRecetas.Web.csproj ^
  -c Release ^
  -o C:\publish\MisRecetas ^
  --self-contained false
```

> **`--self-contained false`** → usa el runtime instalado en el servidor (más liviano).
> Si el servidor **no tiene .NET 9 instalado**, usar `--self-contained true` en su lugar.

La carpeta `C:\publish\MisRecetas` contendrá todo lo necesario para ejecutar la app.

---

## Paso 2 — Configurar `appsettings.json` para producción

En la carpeta publicada, editar `appsettings.json` con los datos reales del servidor:

```json
{
  "ConnectionStrings": {
    "ControlRecetasDB": "Data Source=localhost\\SQLEXPRESS;Database=ControlRecetasDB;User ID=sa;Password=TU_PASSWORD_REAL;Encrypt=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> ⚠️ **No dejar** `appsettings.Development.json` en el servidor — IIS en producción no lo usa (la variable `ASPNETCORE_ENVIRONMENT` no será `Development`).

---

## Paso 3 — Crear el Application Pool en IIS

1. Abrir **Administrador de IIS**
2. Panel izquierdo → **Grupos de aplicaciones** → **Agregar grupo de aplicaciones**
3. Configurar:

| Campo | Valor |
|---|---|
| Nombre | `MisRecetasPool` |
| Versión de .NET CLR | **Sin código administrado** ← importante |
| Modo de canalización | Integrado |

4. Click en el pool recién creado → **Configuración avanzada**:
   - **Identidad** → `ApplicationPoolIdentity` (o una cuenta de servicio específica)
   - **Tiempo de inactividad** → `0` (evita que el pool se apague)
   - **Tiempo máximo de trabajo del proceso** → `0`

---

## Paso 4 — Crear el sitio en IIS

1. Panel izquierdo → **Sitios** → **Agregar sitio web**
2. Configurar:

| Campo | Valor |
|---|---|
| Nombre del sitio | `MisRecetas` |
| Grupo de aplicaciones | `MisRecetasPool` |
| Ruta de acceso física | `C:\publish\MisRecetas` |
| Puerto | `80` (o el que corresponda) |
| Nombre de host | `misrecetas.tudominio.com` (o vacío para localhost) |

---

## Paso 5 — Permisos de carpeta

El pool necesita permiso de **lectura** sobre la carpeta publicada:

```powershell
icacls "C:\publish\MisRecetas" /grant "IIS AppPool\MisRecetasPool:(OI)(CI)RX" /T
```

Si la app necesita escribir logs en disco, agregar también permiso de escritura sobre la carpeta de logs:

```powershell
icacls "C:\publish\MisRecetas\logs" /grant "IIS AppPool\MisRecetasPool:(OI)(CI)M" /T
```

---

## Paso 6 — Verificar `web.config`

El publish genera automáticamente un `web.config`. Verificar que tenga esta estructura:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*"
             modules="AspNetCoreModuleV2"
             resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\MisRecetas.Web.dll"
                  stdoutLogEnabled="false"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

> **`hostingModel="inprocess"`** → mejor rendimiento, el proceso corre dentro de `w3wp.exe`.

Para habilitar logs temporalmente durante debugging:

```xml
stdoutLogEnabled="true"
stdoutLogFile=".\logs\stdout"
```

Crear la carpeta `C:\publish\MisRecetas\logs\` antes de habilitar los logs.

---

## Paso 7 — Permisos de SQL Server

La cuenta del Application Pool necesita acceder a SQL Server. Hay dos opciones:

### Opción A — Autenticación SQL (ya configurado en el proyecto)

La cadena de conexión ya usa `User ID=sa;Password=...` → no se necesita configuración adicional en SQL Server.

### Opción B — Windows Authentication (más seguro)

Cambiar la cadena de conexión a:

```json
"ControlRecetasDB": "Data Source=localhost\\SQLEXPRESS;Database=ControlRecetasDB;Integrated Security=True;TrustServerCertificate=True;"
```

Y dar permisos en SQL Server:

```sql
CREATE LOGIN [IIS AppPool\MisRecetasPool] FROM WINDOWS;

USE ControlRecetasDB;
CREATE USER [IIS AppPool\MisRecetasPool] FOR LOGIN [IIS AppPool\MisRecetasPool];
ALTER ROLE db_datareader ADD MEMBER [IIS AppPool\MisRecetasPool];
ALTER ROLE db_datawriter ADD MEMBER [IIS AppPool\MisRecetasPool];
GRANT EXECUTE TO [IIS AppPool\MisRecetasPool];
```

---

## Paso 8 — Probar el deploy

1. Navegar a `http://localhost` (o el host configurado)
2. Debería aparecer la pantalla de login
3. Si hay error 500.30, habilitar `stdoutLogEnabled="true"` en `web.config` y revisar `logs\stdout_*.log`

### Errores comunes

| Error | Causa probable | Solución |
|---|---|---|
| HTTP 500.19 | `web.config` malformado | Verificar sintaxis XML |
| HTTP 500.30 | .NET 9 runtime no instalado | Instalar Hosting Bundle y hacer `iisreset` |
| HTTP 500.31 | DLL no encontrada | Verificar que `processPath` apunta al `.dll` correcto |
| HTTP 503 | Pool detenido | Verificar Event Viewer → Application Log |
| Login funciona pero DB falla | Credenciales SQL incorrectas | Revisar connection string en `appsettings.json` |

---

## Checklist rápido

```
[ ] .NET 9 Hosting Bundle instalado en el servidor
[ ] IIS reiniciado después de instalar el bundle (iisreset)
[ ] Carpeta publicada con: dotnet publish -c Release
[ ] appsettings.json actualizado con connection string de producción
[ ] appsettings.Development.json eliminado del servidor
[ ] Application Pool creado con "Sin código administrado"
[ ] Sitio web creado apuntando a la carpeta publicada
[ ] Permisos icacls aplicados sobre la carpeta
[ ] SQL Server accesible con las credenciales configuradas
[ ] web.config generado correctamente por el publish
[ ] Carpeta logs/ creada si se habilitan stdoutLogs
[ ] Login exitoso en el navegador
```

---

## Actualizaciones futuras

Para actualizar la aplicación en producción:

```powershell
# 1. Publicar nueva version localmente
dotnet publish MisRecetas.Web/MisRecetas.Web.csproj -c Release -o C:\publish\MisRecetas_new

# 2. Detener el sitio en el servidor
Stop-Website "MisRecetas"

# 3. Copiar archivos (preservar appsettings.json de produccion)
$origen  = "C:\publish\MisRecetas_new\"
$destino = "C:\publish\MisRecetas\"
Get-ChildItem $origen -Recurse |
  Where-Object { $_.Name -notmatch "appsettings\.json$" } |
  Copy-Item -Destination { $_.FullName -replace [regex]::Escape($origen), $destino } -Force

# 4. Reiniciar el sitio
Start-Website "MisRecetas"
```

> ✅ El `appsettings.json` de producción nunca se pisa con este script.
