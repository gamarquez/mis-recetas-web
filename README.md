# MisRecetas.Web

Sistema web de gestión y control de recetas médicas para farmacias, desarrollado en **ASP.NET Core MVC .NET 9**.

---

## ¿De qué trata el proyecto?

MisRecetas.Web es una aplicación interna para farmacias que permite gestionar el ciclo de vida completo de una receta médica: desde su registro en el sistema hasta su entrega al paciente, archivo o desecho.

El sistema cubre las operaciones del día a día del personal de farmacia:

- **Registrar** recetas asociadas a un paciente y un médico
- **Entregar** la receta al médico correspondiente
- **Recibir** la receta de vuelta del médico
- **Entregar** la receta al paciente (o a un familiar autorizado)
- **Archivar** o **desechar** recetas según corresponda
- **Consultar** el estado de cualquier receta en tiempo real
- **Generar reportes** operacionales para seguimiento y auditoría
- **Imprimir tickets** en impresoras térmicas 80mm al entregar al paciente

---

## Tecnologías

| Capa | Tecnología |
|---|---|
| Framework | ASP.NET Core MVC 9.0 |
| Lenguaje | C# 13 |
| Base de datos | SQL Server (Express o full) |
| Acceso a datos | ADO.NET + Stored Procedures |
| Autenticación | Cookie Authentication |
| Hash de passwords | BCrypt.Net-Next 4.0.3 |
| UI | Bootstrap 5.3 + Bootstrap Icons |
| IDE | Visual Studio 2022 / VS Code |
| Plataforma objetivo | Windows Server + IIS |

> No se utiliza Entity Framework — el acceso a datos es 100% ADO.NET con stored procedures, lo que permite máxima compatibilidad con esquemas de base de datos preexistentes.

---

## Funcionalidades

### Gestión de recetas
- Registro de recetas en formulario de 2 pasos (buscar/crear paciente → confirmar receta)
- Alta automática de pacientes nuevos al registrar una receta
- Seguimiento del ciclo completo con fechas y usuarios en cada transición
- Modificación del médico asignado (solo Administrador)
- Búsqueda de recetas por número, paciente o estado

### Ciclo de vida de una receta

```
[1] Registrada
    └─► [2] Entregada al médico
            └─► [3] Recibida del médico
                    └─► [4] Entregada al paciente
                            └─► [5] Archivada
                    └─► [6] Desechada
        └─► [6] Desechada
[5] Archivadas pueden re-entregarse al paciente
```

### Reportes (en ventana popup)
1. **Ticket de receta** — formato térmico 80mm, apto para impresora de punto de venta
2. **Por estado** — listado de recetas filtrado por estado
3. **Archivadas** — con filtro por rango de fechas
4. **Desechadas** — con filtro por rango de fechas
5. **Entregadas por médico** — recetas en estado 2, filtradas por médico
6. **Recibidas de médico** — recetas en estado 3, con días transcurridos
7. **Por médico y fecha** — historial completo de un médico en un período

### Administración
- **Médicos** — ABM completo
- **Pacientes** — ABM con búsqueda por nombre/apellido
- **Usuarios** — ABM con asignación de rol y reset de contraseña

### Seguridad
- Login con usuario y contraseña (BCrypt)
- Sesión por cookie (8 horas, sliding expiration)
- Roles: **Administrador**, **Operador**, **Supervisor**
- Acciones sensibles restringidas por política de autorización
- Antiforgery tokens en todos los formularios POST

---

## Estructura del proyecto

```
MisRecetas.Web/
├── Controllers/        # Lógica de navegación y coordinación
├── Models/             # Entidades del dominio (Receta, Paciente, Médico, etc.)
├── ViewModels/         # Modelos tipados para cada vista
├── Data/               # Repositorios ADO.NET (acceso a BD)
├── Services/           # Lógica de negocio
└── Views/
    ├── Shared/
    │   ├── _Layout.cshtml          # Layout principal con sidebar
    │   ├── _PopupLayout.cshtml     # Layout para reportes en popup
    │   └── _ThermalLayout.cshtml   # Layout para ticket térmico 80mm
    ├── Recetas/        # 9 vistas del flujo de recetas
    ├── Reportes/       # Index + 7 vistas de reportes
    ├── Medicos/        # CRUD médicos
    ├── Pacientes/      # CRUD pacientes
    └── Usuarios/       # CRUD usuarios + reset password
```

---

## Requisitos

- .NET 9 SDK (desarrollo) / .NET 9 Hosting Bundle (producción IIS)
- SQL Server 2019+ o SQL Server Express
- Windows Server 2019+ con IIS (producción)
- Visual Studio 2022 o VS Code

---

## Configuración rápida (desarrollo)

### 1. Clonar el repositorio

```bash
git clone https://github.com/gamarquez/mis-recetas-web.git
cd mis-recetas-web
```

### 2. Configurar la cadena de conexión

Editar `MisRecetas.Web/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "ControlRecetasDB": "Data Source=localhost\\SQLEXPRESS;Database=ControlRecetasDB;User ID=sa;Password=tu_password;Encrypt=True;TrustServerCertificate=True;"
  }
}
```

### 3. Preparar la base de datos

Asegurarse de que existan los siguientes stored procedures en `ControlRecetasDB`:

```sql
-- Combo de médicos para formularios
CREATE PROCEDURE SP_CargarComboBoxMedicos AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id_Medico, Nro_Matricula, Nombre_Medico, Apellido_Medico,
           Apellido_Medico + ', ' + Nombre_Medico AS NombreCompleto
    FROM Medico
    ORDER BY Apellido_Medico, Nombre_Medico
END

-- Combo de tipos de documento
CREATE PROCEDURE SP_CargarComboBoxTipoDocumento AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id_Tipo_Documento, Descripcion
    FROM Tipo_Documento
    ORDER BY Descripcion
END
```

> ⚠️ La columna `Usuario.Password` debe ser `nvarchar(100)` mínimo (BCrypt genera hashes de 60 caracteres).
>
> ```sql
> ALTER TABLE Usuario ALTER COLUMN Password nvarchar(100) NOT NULL;
> ```

### 4. Ejecutar

```bash
cd MisRecetas.Web
dotnet run
```

La app estará disponible en `https://localhost:7102` o `http://localhost:5240`.

---

## Deploy en IIS

Ver la guía completa en [`DEPLOY-IIS.md`](DEPLOY-IIS.md).

Resumen:

```bash
# Publicar
dotnet publish MisRecetas.Web/MisRecetas.Web.csproj -c Release -o C:\publish\MisRecetas

# En IIS:
# - Instalar .NET 9 Hosting Bundle + iisreset
# - Crear Application Pool sin código administrado
# - Crear sitio apuntando a C:\publish\MisRecetas
# - Configurar appsettings.json con datos de producción
```

---

## Documentación adicional

| Archivo | Descripción |
|---|---|
| [`MEJORAS.md`](MEJORAS.md) | Detalle de todas las mejoras realizadas en la migración |
| [`DEPLOY-IIS.md`](DEPLOY-IIS.md) | Guía paso a paso para deploy en Windows Server + IIS |

---

## Licencia

Uso interno. Todos los derechos reservados.
