using System;
using Controladores;
using Logica;
using SistemaProveedores.Datos;

namespace SistemaProveedores.Config
{
    public static class ContenedorDependencias
    {
        public static AuthController Auth { get; private set; }
        public static UsuarioController Usuarios { get; private set; }
        public static CategoriaController Categorias { get; private set; }
        public static ProductoController Productos { get; private set; }
        public static ProveedorController Proveedores { get; private set; }
        public static ClasificacionController Clasificacion { get; private set; }
        public static ConsultasController Consultas { get; private set; }
        public static ReportesController Reportes { get; private set; }
        public static BitacoraController Bitacora { get; private set; }

        public static void Inicializar(string cadenaConexion)
        {
            var conexion = new ConexionBD(cadenaConexion);

            var usuarioCD = new UsuarioCD(conexion);
            var bitacoraCD = new BitacoraCD(conexion);
            var rolCD = new RolUsuarioCD(conexion);
            var categoriaCD = new CategoriaCD(conexion);
            var productoCD = new ProductoCD(conexion);
            var proveedorCD = new ProveedorCD(conexion);
            var clasificacionCD = new ClasificacionProductoCD(conexion);
            var consultasCD = new ConsultasCD(conexion);
            var reportesCD = new ReportesCD(conexion);
            var reportesFiltradosCD = new ReportesFiltradosCD(conexion);

            Auth = new AuthController(new AuthLN(usuarioCD, bitacoraCD));
            Usuarios = new UsuarioController(new UsuarioLN(usuarioCD), new RolUsuarioLN(rolCD));
            Categorias = new CategoriaController(new CategoriaLN(categoriaCD));
            Productos = new ProductoController(new ProductoLN(productoCD));
            Proveedores = new ProveedorController(new ProveedorLN(proveedorCD));
            Clasificacion = new ClasificacionController(new ClasificacionProductoLN(clasificacionCD));
            Consultas = new ConsultasController(new ConsultasLN(consultasCD));
            Reportes = new ReportesController(new ReportesFiltradosLN(reportesFiltradosCD));
            Bitacora = new BitacoraController(new ReportesLN(reportesCD));
        }
    }
}