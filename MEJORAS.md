# MisRecetas.Web — Documentacion de mejoras y estructura del proyecto

## Resumen del proyecto

**MisRecetas.Web** es una aplicacion ASP.NET Core MVC (.NET 9) para la gestion de recetas medicas en una farmacia. Permite registrar recetas, seguir su ciclo de vida (registro → entrega a medico → recepcion de medico → entrega a paciente → archivo o desecho) y generar reportes.

---

## Tecnologias utilizadas

| Componente | Tecnologia |
|---|---|
| Framework | ASP.NET Core MVC 9.0 |
| Lenguaje | C# 13 |
| Base de datos | SQL Server 2025 Express (`localhost\SQLEXPRESS`) |
| Acceso a datos | ADO.NET puro (sin Entity Framework) |
| Autenticacion | Cookie Authentication |
| Hashing de passwords | BCrypt.Net-Next 4.0.3 |
| UI | Bootstrap 5.3 + Bootstrap Icons |
| Solucion VS | MisRecetas.sln / MisRecetas.slnx |

---

## Estructura del proyecto

```
mis-recetas-web/
├── MisRecetas.sln              # Solucion Visual Studio clasico
├── MisRecetas.slnx             # Solucion Visual Studio moderno (XML)
├── .gitignore                  # Exclusiones estandar .NET
└── MisRecetas.Web/
    ├── MisRecetas.Web.csproj   # Proyecto web
    ├── Program.cs              # Configuracion de la app (DI, Auth, rutas)
    ├── appsettings.json        # Configuracion base
    ├── appsettings.Development.json  # Cadena de conexion desarrollo
    ├── Properties/
    │   └── launchSettings.json # Perfiles de ejecucion (VS / dotnet run)
    │
    ├── Models/                 # Modelos de dominio (mapean tablas DB)
    │   ├── Medico.cs
    │   ├── Paciente.cs
    │   ├── Receta.cs
    │   ├── RecetaDetalle.cs    # Vista enriquecida con datos de reportes
    │   ├── Usuario.cs
    │   ├── Rol.cs
    │   ├── Estado.cs
    │   └── TipoDocumento.cs
    │
    ├── Data/                   # Repositorios (acceso a DB via ADO.NET)
    │   ├── DbConnectionFactory.cs      # Crea SqlConnection desde config
    │   ├── UsuarioRepository.cs        # CRUD usuarios + autenticacion
    │   ├── MedicoRepository.cs         # CRUD medicos
    │   ├── PacienteRepository.cs       # CRUD pacientes (busqueda por doc)
    │   ├── RecetaRepository.cs         # Ciclo de vida de recetas
    │   └── CatalogoRepository.cs       # Catalogos: estados, roles, tipo doc
    │
    ├── Services/               # Logica de negocio
    │   ├── AuthService.cs              # Login, cambio de password (BCrypt)
    │   ├── MedicoService.cs            # Gestión medicos
    │   ├── PacienteService.cs          # Gestion pacientes
    │   ├── RecetaService.cs            # Flujo de recetas
    │   ├── ReporteService.cs           # Consultas de reportes
    │   └── UsuarioService.cs           # Gestion usuarios
    │
    ├── Controllers/            # Controladores MVC
    │   ├── AccountController.cs        # Login, logout, cambio password
    │   ├── DashboardController.cs      # Pantalla principal con estadisticas
    │   ├── RecetasController.cs        # Flujo completo de recetas
    │   ├── MedicosController.cs        # CRUD medicos
    │   ├── PacientesController.cs      # CRUD pacientes
    │   ├── UsuariosController.cs       # CRUD usuarios + reset password
    │   ├── ReportesController.cs       # 7 reportes operacionales
    │   └── HomeController.cs           # Redireccion inicial
    │
    ├── ViewModels/             # ViewModels tipados para cada vista
    │   ├── LoginViewModel.cs
    │   ├── DashboardViewModel.cs
    │   ├── RegistrarRecetaViewModel.cs
    │   ├── BuscarRecetaViewModel.cs
    │   ├── EntregarAMedicoViewModel.cs
    │   ├── RecibirDeMedicoViewModel.cs
    │   ├── EntregarAPacienteViewModel.cs
    │   ├── ArchivarRecetasViewModel.cs
    │   ├── DesecharRecetasViewModel.cs
    │   ├── EntregarArchivadasViewModel.cs
    │   ├── ModificarRecetaViewModel.cs
    │   ├── MedicoViewModel.cs
    │   ├── PacienteViewModel.cs
    │   ├── UsuarioViewModel.cs
    │   ├── ReporteViewModel.cs         # Compartido por todos los reportes
    │   └── CambiarPasswordViewModel.cs
    │
    └── Views/
        ├── _ViewImports.cshtml         # @addTagHelper, @using globales
        ├── _ViewStart.cshtml           # Layout por defecto = _Layout
        ├── Shared/
        │   ├── _Layout.cshtml          # Layout principal con sidebar
        │   ├── _PopupLayout.cshtml     # Layout para ventanas popup (reportes)
        │   ├── _ThermalLayout.cshtml   # Layout para ticket termico 80mm
        │   ├── _ValidationScriptsPartial.cshtml
        │   └── Error.cshtml
        ├── Account/           # Login, AccesoDenegado, CambiarPassword
        ├── Dashboard/         # Index con KPIs y ultimas recetas
        ├── Recetas/           # Registrar, Buscar, EntregarAMedico,
        │                      # RecibirDeMedico, EntregarAPaciente,
        │                      # Archivar, Desechar, EntregarArchivadas, Modificar
        ├── Medicos/           # Index, Crear, Editar
        ├── Pacientes/         # Index, Crear, Editar
        ├── Usuarios/          # Index, Crear, Editar, ResetPassword
        └── Reportes/          # Index + 7 vistas de reporte
```

---

## Base de datos

**Nombre:** `ControlRecetasDB`
**Servidor:** `localhost\SQLEXPRESS`
**Autenticacion:** SQL Server (`sa`)

### Tablas principales

| Tabla | Descripcion |
|---|---|
| `Estado` | 6 estados del ciclo de receta (1=Registrada … 6=Desechada) |
| `Rol` | 3 roles: Administrador, Operador, Supervisor |
| `Tipo_Documento` | DNI, Pasaporte, LC, LE |
| `Medico` | Medicos con matricula |
| `Paciente` | Pacientes con tipo+nro documento y email |
| `Receta` | Receta con todas las fechas de transicion de estado |
| `Usuario` | Usuarios del sistema con password BCrypt (`nvarchar(100)`) |

### Stored procedures importantes

| SP | Uso |
|---|---|
| `SP_CargarComboBoxMedicos` | Devuelve `Id_Medico, Nro_Matricula, Nombre_Medico, Apellido_Medico, NombreCompleto` |
| `SP_CargarComboBoxTipoDocumento` | Devuelve `Id_Tipo_Documento, Descripcion` |

> **Nota:** La columna `Usuario.Password` fue alterada de `nvarchar(50)` a `nvarchar(100)` para soportar hashes BCrypt (60 caracteres).

---

## Flujo de estados de una receta

```
[1] Registrada
    └─► [2] Entregada al medico   (EntregarAMedico)
            └─► [3] Recibida del medico  (RecibirDeMedico)
                    └─► [4] Entregada al paciente  (EntregarAPaciente)
                            └─► [5] Archivada  (Archivar)
                    └─► [6] Desechada  (Desechar)
        └─► [6] Desechada  (Desechar)
[5] Archivadas pueden re-entregarse  (EntregarArchivadas)
```

---

## Mejoras realizadas

### 1. Creacion completa del proyecto desde cero

El proyecto fue migrado/creado desde cero como una aplicacion ASP.NET Core MVC .NET 9, reemplazando la version anterior (.NET Framework / Web Forms). Se implementaron:

- Arquitectura en capas: Models → Data (repositorios) → Services → Controllers → Views
- Autenticacion por cookies con roles (Administrador, Operador, Supervisor)
- Hashing de passwords con BCrypt.Net-Next
- ADO.NET puro, sin Entity Framework, usando stored procedures del SQL Server preexistente

### 2. Solucion de Visual Studio

Se crearon los archivos de solucion que faltaban:
- `MisRecetas.sln` — formato clasico compatible con todas las versiones de VS
- `MisRecetas.slnx` — formato moderno (SDK-style, Visual Studio 2022+)
- `.gitignore` estandar .NET para excluir `bin/`, `obj/`, `.vs/`, etc.

### 3. Correcciones de base de datos

| Problema | Solucion |
|---|---|
| `Usuario.Password` era `nvarchar(50)` | Alterada a `nvarchar(100)` para soportar BCrypt (60 chars) |
| `SP_CargarComboBoxMedicos` devolvía solo `NombreCompleto` concatenado | Recreada para devolver `Id_Medico`, `Nro_Matricula`, `Nombre_Medico`, `Apellido_Medico`, `NombreCompleto` |
| `SP_CargarComboBoxTipoDocumento` no existia | Creada desde cero |

### 4. Correccion del flujo de Registrar Receta (formulario en 2 pasos)

**Problema:** Al completar el paso 1 (buscar paciente), se mostraban errores de validacion de los campos del paso 2 (medico, email, nro receta).

**Solucion:** En `RecetasController.BuscarPaciente()` se llama a `ModelState.Remove()` para cada campo del paso 2 antes de validar, evitando la contaminacion entre pasos.

**Problema adicional:** Despues de registrar una receta, no se ofrecia imprimir el ticket.

**Solucion:** Se guarda `TempData["NroRecetaRegistrada"]` en `ConfirmarRegistro()` y la vista `Registrar.cshtml` muestra un banner de exito con boton para abrir el ticket en popup.

### 5. Reportes en ventana popup

**Antes:** Los reportes se abrían en la misma ventana o en una nueva pestaña del navegador (`target="_blank"`).

**Ahora:** Todos los reportes se abren en una **ventana popup** centrada en pantalla mediante `window.open()` con dimensiones especificas:

```javascript
function abrirReporte(url, ancho, alto) {
    var left = Math.round((screen.width - ancho) / 2);
    var top  = Math.round((screen.height - alto)  / 2);
    window.open(url, 'reporte',
        'width=' + ancho + ',height=' + alto +
        ',left=' + left + ',top=' + top +
        ',resizable=yes,scrollbars=yes,toolbar=no,menubar=no');
}
```

Cada reporte tiene su propio tamano de popup segun la cantidad de columnas que muestra.

El mismo mecanismo aplica al ticket desde la vista Registrar.

### 6. Layouts especializados para reportes

Se crearon dos layouts nuevos en `Views/Shared/`:

#### `_PopupLayout.cshtml`
Layout minimalista para reportes tabulares en popup. Incluye:
- Barra superior oscura (`#1a2942`) con titulo del reporte
- Boton "Imprimir" (`window.print()`)
- Boton "Cerrar" (`window.close()`)
- Estilos `@media print` que ocultan la barra y el fondo

#### `_ThermalLayout.cshtml`
Layout optimizado para **impresora termica 80mm** (ticket de receta). Caracteristicas:
- Ancho fijo de 302px en pantalla (equivale a 80mm a 96 DPI)
- Fuente monoespaciada (`Courier New`) tipica de impresoras termicas
- `@media print` ajusta a `72mm` con padding minimo y tipografia de 9pt
- Separadores `---` con `border-top: 1px dashed`
- Numero de receta en tipografia grande (42px en pantalla / 32pt en impresion)
- Secciones: encabezado, numero de receta, paciente, medico, fechas, pie

### 7. Ticket de receta optimizado para impresion termica 80mm

**Antes:** El ticket mostraba tablas Bootstrap con muchas columnas, inadecuado para papel termico angosto.

**Ahora:** El ticket usa el `_ThermalLayout` y muestra un formato compacto tipo punto de venta:

```
┌─────────────────────────────┐
│     MIS RECETAS             │
│  Farmacia Control de Recetas│
├─────────────────────────────┤
│          Nro. Receta        │
│             1042            │
│          [REGISTRADA]       │
├─ - - - - - - - - - - - - - ┤
│  Paciente: GARCIA, JUAN     │
│  Doc.:     DNI 30123456     │
├─ - - - - - - - - - - - - - ┤
│  Medico:   PEREZ, CARLOS    │
│  Matricula: 12345           │
├─ - - - - - - - - - - - - - ┤
│  Registro: 20/02/2026 10:30 │
├─────────────────────────────┤
│ Impreso: 20/02/2026 10:45   │
└─────────────────────────────┘
```

**Datos mostrados:**
- Numero de receta (grande, centrado)
- Estado actual
- Paciente completo y tipo/numero de documento
- Medico completo y matricula
- Fecha de registro
- Fechas de transicion (entrega medico, recepcion medico, entrega paciente) — solo si aplican
- Nombre de quien retira y parentesco (si aplica)
- Fecha de impresion

### 8. Filtros de fecha en reportes Archivadas y Desechadas

**Antes:** Los reportes no tenian parametros, devolvian todos los registros sin filtro.

**Ahora:** Aceptan `DateTime? fechaDesde` y `DateTime? fechaHasta` y aplican filtrado en memoria sobre el listado completo.

### 9. Correccion NullReferenceException en vista Ticket

**Problema:** Al abrir `/Reportes/Ticket` sin parametro de numero de receta, el controlador retornaba `View((object?)null)` pero la vista declaraba `@model ReporteViewModel` y accedia a `Model.NroReceta`.

**Solucion:** El controlador siempre retorna un `new ReporteViewModel()`, nunca null. La vista verifica `Model.NroReceta == null` para mostrar el formulario de busqueda.

### 10. Parametro del formulario Ticket corregido

**Problema:** El input del formulario de busqueda tenia `name="nroReceta"` pero el controlador esperaba el parametro `int? nro`.

**Solucion:** Cambiado a `name="nro"` para coincidir con el parametro del action.

---

## Como abrir el proyecto en Visual Studio

1. Abrir `MisRecetas.sln` (o `MisRecetas.slnx` en VS 2022+)
2. El proyecto `MisRecetas.Web` aparecera en el Explorador de soluciones
3. Verificar que `appsettings.Development.json` tenga la cadena de conexion correcta:
   ```json
   {
     "ConnectionStrings": {
       "ControlRecetasDB": "Data Source=localhost\\SQLEXPRESS;Database=ControlRecetasDB;User ID=sa;Password=<tu_password>;Encrypt=True;TrustServerCertificate=True;"
     }
   }
   ```
4. Ejecutar con F5 o `dotnet run` desde `MisRecetas.Web/`
5. La app abre en `https://localhost:5001` (o `http://localhost:5000`)

---

## Como ejecutar desde linea de comandos

```bash
cd MisRecetas.Web
dotnet run
```

O para produccion:
```bash
dotnet publish -c Release
```

---

## Usuarios por defecto

Los usuarios se encuentran en la tabla `Usuario` de la base de datos. Los passwords estan hasheados con BCrypt. Para crear o resetear un usuario desde la aplicacion usar el modulo **Usuarios > Restablecer contrasena**.

---

## Notas para el desarrollador

- El proyecto usa **ADO.NET puro** — no hay DbContext ni migraciones. Los cambios de esquema deben hacerse directamente en SQL Server.
- Los stored procedures deben existir en la base antes de iniciar la aplicacion.
- La columna `Usuario.Password` debe ser `nvarchar(100)` minimo.
- `appsettings.Development.json` NO debe commitearse con credenciales reales en entornos compartidos — usar variables de entorno o secrets en produccion.
- La rama principal del repositorio es `main`; el desarrollo se hace en branches con prefijo `claude/`.
