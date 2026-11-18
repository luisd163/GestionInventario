# GestionInventario

Aplicación de escritorio para la gestión de inventario, desarrollada en **C#** con **Windows Forms** y **SQL Server**. Permite controlar entidades, registrar transacciones, consultar información y generar reportes, con control de acceso por tipo de usuario y bitácora de auditoría.

## Características

- **Ingreso al sistema** con autenticación de usuarios.
- **Entidades**: mantenimiento de los datos maestros (<productos, categorías, proveedores, etc.>).
- **Transacciones**: registro de movimientos de inventario (<entradas, salidas, ajustes>).
- **Consultas y Reportes**: búsqueda de información y reportes del inventario.
- **Auditoría**: gestión de usuarios y bitácora de entradas y salidas del sistema.

## Roles de usuario

| Rol | Permisos |
|---|---|
| **Administrador** | Acceso total. Es el único que puede crear usuarios. |
| **Tradicional** | Puede crear, consultar, modificar y eliminar datos (CRUD). |
| **Esporádico** | Solo puede realizar consultas. |

## Tecnologías

- C# / .NET (<versión, p. ej. .NET Framework 4.8>)
- Windows Forms
- SQL Server y SQL Server Management Studio (SSMS)
- Visual Studio (<versión, p. ej. 2022>)

## Requisitos previos

- Windows 10 o superior
- Visual Studio con la carga de trabajo **Desarrollo de escritorio con .NET**
- SQL Server (Express o superior) y SSMS

## Instalación

1. **Clona el repositorio**
   ```bash
   git clone https://github.com/luisd163/GestionInventario.git
   cd GestionInventario
   ```

2. **Crea la base de datos**
   - Abre SSMS y conéctate a tu instancia de SQL Server.
   - Ejecuta el script `<ruta/al/script.sql>` para crear las tablas y los datos iniciales.

3. **Configura la cadena de conexión**
   - Abre `<App.config / archivo donde está la conexión>` y ajusta el servidor y la base de datos:
   ```xml
   <connectionStrings>
     <add name="<NombreConexion>"
          connectionString="Server=<TU_SERVIDOR>;Database=<TU_BASE_DE_DATOS>;Integrated Security=True;" />
   </connectionStrings>
   ```

4. **Compila y ejecuta**
   - Abre `GestionInventario.sln` en Visual Studio.
   - Restaura los paquetes NuGet si es necesario.
   - Presiona `F5` para ejecutar.

## Primer acceso

Usuario administrador por defecto: `<usuario>` / `<contraseña>`
(Cámbiala después del primer ingreso.)

## Estructura del proyecto

```
GestionInventario/
├── <Forms/>           # Pantallas de Windows Forms
├── <Datos/>           # Acceso a datos (SQL Server)
├── <Modelos/>         # Clases de entidades
├── <Scripts/>         # Scripts SQL
└── GestionInventario.sln
```

## Capturas de pantalla

<!-- Agrega aquí imágenes de la pantalla de ingreso y los módulos -->

## Autor

**Luis** — [@luisd163](https://github.com/luisd163)
