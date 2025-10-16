# ParkingZone

Sistema de gestión de estacionamientos (parking) desarrollado con ASP.NET Core (Razor Pages) y Entity Framework Core. Permite administrar parkings, espacios, usuarios, vehículos y reservas, con vistas diferenciadas para administradores, trabajadores y clientes.

## Objetivo del proyecto

Centralizar la operación de un estacionamiento:
- Registro y administración de parkings y sus espacios.
- Gestión de usuarios y vehículos.
- Creación y seguimiento de reservas y pagos.
- Interfaces separadas por rol: Admin, Worker y Client.

## Características principales

- Administración completa de catálogos: `Parking`, `Space`, `User`, `Vehicle` (CRUD).
- Flujo de reservas para clientes (`Client/Reservation`).
- Páginas y layouts específicos por rol:
  - `Admin`: gestión avanzada de entidades.
  - `Worker`: operaciones del día a día.
  - `Client`: navegación y reservas.
- Persistencia con Entity Framework Core y migraciones incluidas.
- Estilos con Bootstrap y assets en `wwwroot`.

## Stack técnico

- **Backend/UI**: ASP.NET Core Razor Pages
- **ORM**: Entity Framework Core
- **Base de datos**: configurable vía `appsettings.json` (por defecto SQL Server LocalDB u otro provisto)
- **Lenguaje**: C# (.NET)
- **Frontend**: Bootstrap, jQuery y validaciones unobtrusive

## Estructura del proyecto

Raíz de la solución:
- `ParkingZone.sln`: solución de Visual Studio/.NET

Proyecto `ParkingZone/`:
- `Program.cs`: arranque y configuración de servicios.
- `appsettings.json`: configuración (cadena de conexión, etc.).
- `ParkingZone.csproj`: configuración del proyecto.
- `Properties/launchSettings.json`: perfiles de ejecución.
- `DbContext/ParkingZoneContext.cs`: contexto EF Core.
- `Migrations/`: migraciones generadas y `ParkingZoneContextModelSnapshot.cs`.
- `Models/`: entidades de dominio
  - `Parking.cs`, `Space.cs`, `User.cs`, `Vehicle.cs`, `Reservation.cs`, `Payment.cs`, `Enums.cs`.
- `Pages/`: Razor Pages
  - `Admin/`: CRUD de `Parking`, `Space`, `User`, `Vehicle` con sus páginas `Create/Edit/Delete/Details/Index`.
  - `Client/`: `Index`, `Reservation`.
  - `Worker/`: `Index`.
  - `Account/`: `Login`, `Logout`.
  - `Shared/`: layouts (`_Layout.cshtml`, `_Layout_Admin.cshtml`, `_Layout_Client.cshtml`, `_Layout_Worker.cshtml`) y estilos asociados.
  - Páginas generales: `Index`, `Privacy`, `Error`.
- `wwwroot/`: recursos estáticos (css, js, librerías).

## Modelos principales

- **Parking**: información del estacionamiento (nombre, ubicación, etc.).
- **Space**: espacio/lugar dentro de un parking; suele estar asociado a un `Parking` y su estado/disponibilidad.
- **User**: usuarios del sistema; roles asociados (ver `Enums.cs`).
- **Vehicle**: vehículos de los usuarios (patente/matrícula, tipo, etc.).
- **Reservation**: reserva de un `Space` por un `User` (cliente) para un `Vehicle` en un rango horario.
- **Payment**: registro de pagos asociados a reservas.
- **Enums**: enumeraciones de apoyo (por ejemplo, roles o estados).

Nota: Revise los archivos en `Models/` para los campos exactos y restricciones.

## Migraciones y base de datos

Las migraciones existentes están en `Migrations/`:
- `20251007050443_first*`
- `20251007061148_two*`

Para aplicar la base de datos localmente:

```bash
dotnet restore
dotnet tool install --global dotnet-ef --version 9.*  # si no lo tienes
dotnet ef database update --project ParkingZone/ParkingZone.csproj
```

Si necesita crear una nueva migración:

```bash
dotnet ef migrations add <NombreMigracion> --project ParkingZone/ParkingZone.csproj
dotnet ef database update --project ParkingZone/ParkingZone.csproj
```

## Configuración

Edite `ParkingZone/appsettings.json` para definir la cadena de conexión bajo `ConnectionStrings`.
Ejemplo (SQL Server LocalDB):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ParkingZone;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

## Ejecución local

Desde la raíz del repositorio o dentro de `ParkingZone/`:

```bash
dotnet restore
dotnet build ParkingZone/ParkingZone.csproj
dotnet run --project ParkingZone/ParkingZone.csproj
```

El proyecto levantará un servidor Kestrel; los perfiles de `launchSettings.json` también permiten ejecutar desde Visual Studio.

## Autenticación y roles

El proyecto incluye páginas de `Login` y `Logout` en `Pages/Account/`. Los layouts (`Admin`, `Client`, `Worker`) sugieren separación por rol. Asegúrese de implementar/ajustar la autorización en `Program.cs` y en cada página según su necesidad.

## Estilos y UI

- Bootstrap y jQuery se encuentran en `wwwroot/lib/`.
- Estilos personalizados en `wwwroot/css/site.css` y en los `.css` asociados a cada layout en `Pages/Shared/`.

## Scripts útiles

```bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build ParkingZone/ParkingZone.csproj

# Ejecutar
dotnet run --project ParkingZone/ParkingZone.csproj

# Migraciones
dotnet ef migrations add Initial
dotnet ef database update
```

## Roadmap (ideas)

- Validaciones y reglas de negocio avanzadas para reservas y pagos.
- Reportes y dashboard para `Admin`.
- Integración de pasarela de pagos.
- Notificaciones por correo/SMS.

## Licencia

Este proyecto se distribuye con fines educativos. Ajuste la licencia según sus necesidades.

