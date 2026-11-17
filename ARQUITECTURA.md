## 1. Resumen de lo que ya existe (10 líneas)

1. Capas por carpeta: `Modelo/` (entidades), `Datos/` (DAO: `*CD`), `Logica/` (`*LN` + interfaces `I*Datos`), `Config/` (`ConexionBD`, `SesionUsuario`).
2. Modelo: `Usuario`, `RolUsuario` (props privadas, sin uso), `Bitacora`, `Categoria`, `Producto`, `Proveedor`, `CatalogoProveedor`, `ClasificacionProducto`.
3. DAO con ADO.NET puro y parámetros: `UsuarioCD` (también hace de login), `BitacoraCD` (código base-36 con transacción), `ProductoCD`, `ProveedorCD`, `CategoriaCD`, `CatalogoProveedorCD`, `SqlDatosHelper`.
4. `ReportesCD` ya tiene: `ProductosDeProveedor`, `ProveedoresDeProducto`, `ProductosPorCategoria`, `CategoriasPorNombreProducto` y `Generar(TipoReporte)`, con `Bitacora` por fechas.
5. `ConsultasCD` tiene 5 consultas (3 JOIN, 2 con EXISTS/IN) que **no coinciden** con las 5 requeridas en el documento (§4.1).
6. Lógica con RBAC: `ReglasLN` (ADM/TRD/ESP), `AuthLN` (login + bitácora entrada/salida), `UsuarioLN` (solo ADM), `ProductoLN`, `ProveedorLN` (incluye catálogo), `CategoriaLN`, `ReportesLN`.
7. `SesionUsuario` es estática, con código, nombre, rol y bitácora.
8. `Datos/Program.cs` es una consola de prueba de conexión (cadena `localhost\SQLEXPRESS`, BD `BDGestionInventario`). Tiene el comentario "aquí inicia la interfaz".
9. No existe ninguna vista WinForms, ningún controlador, ningún DAO de `RolUsuario` ni de `ClasificacionProducto` (M:N), ni las 3 consultas JOIN, 2 subconsultas y 5 reportes del PDF.
10. Problemas a corregir:
    - El `.csproj` es `net10.0 Exe` sin Windows Forms.
    - Namespaces mezclados (`Config` vs `SistemaProveedores.Config`).
    - Nombres de tabla en SQL con espacios (`[Categoria Producto]`, `[Catálogo Proveedor]`) frente al diccionario.
    - `CaCantidad` se valida `> 0` y el diccionario dice `>= 0`.
    - `Producto.Categoria` (FK directa) coexiste con la tabla M:N `ClasificacionProducto`.
    - Contraseña en texto plano.

## 2. Árbol de carpetas y archivos

```
GestionInventario/
├─ GestionInventario.csproj          (MODIFICAR: net8.0-windows, UseWindowsForms)
├─ Program.cs                        (MOVER desde Datos/; Application.Run(new frmLogin))
├─ Config/
│  ├─ ConexionBD.cs                  (existe)
│  ├─ SesionUsuario.cs               (existe)
│  └─ ContenedorDependencias.cs      (NUEVO: arma DAO→LN→Controladores)
├─ Modelo/
│  ├─ (8 clases existentes)
│  └─ DTO/
│     ├─ OfertaProductoDTO.cs           (Consulta 1)
│     ├─ AuditoriaAccesoDTO.cs          (Consulta 2)
│     ├─ DistribucionProveedorDTO.cs    (Consulta 3)
│     ├─ ProveedorPrecioSobrePromDTO.cs (Subconsulta 1)
│     ├─ ProductoSinOfertaDTO.cs        (Subconsulta 2)
│     └─ ResumenCategoriaDTO.cs         (Reporte multicategoría)
├─ Datos/
│  ├─ (DAO existentes)
│  ├─ RolUsuarioCD.cs                (NUEVO)
│  ├─ ClasificacionProductoCD.cs     (NUEVO)
│  ├─ ConsultasCD.cs                 (REEMPLAZAR: 5 consultas requeridas)
│  └─ ReportesCD.cs                  (AMPLIAR: ProveedoresPorCiudad, ComparativaPrecios, MultiCategoria)
├─ Logica/
│  ├─ (LN e interfaces existentes)
│  ├─ RolUsuarioLN.cs                (NUEVO)
│  ├─ ClasificacionProductoLN.cs     (NUEVO)
│  ├─ ConsultasLN.cs                 (NUEVO)
│  └─ ContratosDatos.cs              (AMPLIAR: IRolUsuarioDatos, IClasificacionDatos, IConsultasDatos)
├─ Controladores/
│  ├─ AuthController.cs
│  ├─ UsuarioController.cs
│  ├─ CategoriaController.cs
│  ├─ ProductoController.cs
│  ├─ ProveedorController.cs
│  ├─ ClasificacionController.cs
│  ├─ ConsultasController.cs
│  ├─ ReportesController.cs
│  └─ BitacoraController.cs
├─ Vistas/
│  ├─ frmLogin.cs (+ .Designer.cs)
│  ├─ frmPrincipal.cs (MDI/menú por rol)
│  ├─ frmUsuarios.cs
│  ├─ frmCategorias.cs
│  ├─ frmProductos.cs
│  ├─ frmProveedores.cs
│  ├─ frmCatalogoProveedor.cs
│  ├─ frmClasificacion.cs
│  ├─ frmConsultas.cs
│  ├─ frmReportes.cs
│  ├─ frmBitacora.cs
│  └─ Util/ExportadorPdf.cs, VisorReporte.cs
```

## 3. Contrato de firmas de los controladores

```csharp
namespace Controladores
{
    public sealed class AuthController
    {
        public bool IniciarSesion(string uCodigo, string uContrasena);
        public void CerrarSesion();
        public string RolActual();
        public bool PuedeEscribirCatalogos();
        public bool EsAdministrador();
    }

    public sealed class UsuarioController
    {
        public IList<Usuario> Listar();
        public Usuario ObtenerPorCodigo(string uCodigo);
        public void Crear(string uNombre, string uCodigo, string uContrasena, string codigoRol);
        public void Actualizar(string uNombre, string uCodigo, string uContrasena, string codigoRol);
        public void Eliminar(string uCodigo);
        public IList<RolUsuario> ListarRoles();
    }

    public sealed class CategoriaController
    {
        public IList<Categoria> Listar();
        public Categoria ObtenerPorCodigo(string cCodigo);
        public void Crear(string cCodigo, string cNombre);
        public void Actualizar(string cCodigo, string cNombre);
        public void Eliminar(string cCodigo);
    }

    public sealed class ProductoController
    {
        public IList<Producto> Listar();
        public Producto ObtenerPorCodigo(string prCodigo);
        public void Crear(string prCodigo, string prNombre, string prDescripcion, string codigoCategoria);
        public void Actualizar(string prCodigo, string prNombre, string prDescripcion, string codigoCategoria);
        public void Eliminar(string prCodigo);
    }

    public sealed class ProveedorController
    {
        public IList<Proveedor> Listar();
        public Proveedor ObtenerPorCodigo(string pCodigo);
        public void Crear(string pCodigo, string pNombre, string pCiudad, string pDireccion);
        public void Actualizar(string pCodigo, string pNombre, string pCiudad, string pDireccion);
        public void Eliminar(string pCodigo);
        public IList<CatalogoProveedor> ObtenerCatalogo(string pCodigo);
        public void AgregarProductoCatalogo(string pCodigo, string prCodigo, int cantidad, decimal precio);
        public void ActualizarProductoCatalogo(string pCodigo, string prCodigo, int cantidad, decimal precio);
        public void EliminarProductoCatalogo(string pCodigo, string prCodigo);
    }

    public sealed class ClasificacionController
    {
        public IList<ClasificacionProducto> ListarPorProducto(string prCodigo);
        public void Asignar(string prCodigo, string cCodigo);
        public void Quitar(string prCodigo, string cCodigo);
    }

    public sealed class ConsultasController
    {
        public IList<OfertaProductoDTO> CatalogoUnificadoOferta();               // Consulta 1
        public IList<AuditoriaAccesoDTO> AuditoriaAccesosPorUsuario();           // Consulta 2
        public IList<DistribucionProveedorDTO> DistribucionProveedoresFamilia(); // Consulta 3
        public IList<ProveedorPrecioSobrePromDTO> ProveedoresPrecioSobrePromedio(); // Subconsulta 1
        public IList<ProductoSinOfertaDTO> ProductosSinOferta();                 // Subconsulta 2
    }

    public sealed class ReportesController
    {
        public IList<string> CiudadesDisponibles();
        public DataTable ProveedoresPorCiudad(string ciudad);                    // Reporte 1
        public DataTable CoberturaPorProveedor(string pCodigo);                  // Reporte 2
        public DataTable ComparativaPreciosProducto(string prCodigo);            // Reporte 3 (ASC por precio)
        public DataTable CatalogoPorCategoria(string cCodigo);                   // Reporte 4
        public IList<ResumenCategoriaDTO> MultiCategoriaDisponibilidad(IList<string> codigosCategoria); // Reporte 5
        public void ExportarPdf(DataTable datos, string titulo, string rutaArchivo);
    }

    public sealed class BitacoraController
    {
        public DataTable Listar(DateTime? desde, DateTime? hasta);   // solo ADM
    }
}
```
