using System;
using System.Collections.Generic;
using System.Data;
using Modelo;

namespace Logica
{
    public sealed class UsuarioAutenticadoDatos
    {
        public string CodigoUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string CodigoRol { get; set; }
    }

    public enum TipoReporte
    {
        Inventario,
        Proveedores,
        Categorias,
        Bitacora
    }

    public interface IAuthDatos
    {
        UsuarioAutenticadoDatos ValidarCredenciales(string identificador, string UContrasena);
    }

    public interface IBitacoraDatos
    {
        string RegistrarEntrada(string codigoUsuario, DateTime fechaHoraEntrada);
        void RegistrarSalida(string codigoBitacora, DateTime fechaHoraSalida);
    }

    public interface IUsuarioDatos
    {
        IList<Usuario> Listar();
        Usuario ObtenerPorCodigo(string codigo);
        void Crear(Usuario usuario, string UContrasena);
        void Actualizar(Usuario usuario, string UContrasena);
        void Eliminar(string codigo);
    }

    public interface IProductoDatos
    {
        IList<Producto> Listar();
        Producto ObtenerPorCodigo(string codigo);
        void Crear(Producto producto);
        void Actualizar(Producto producto);
        void Eliminar(string codigo);
    }

    public interface IProveedorDatos
    {
        IList<Proveedor> Listar();
        Proveedor ObtenerPorCodigo(string codigo);
        void Crear(Proveedor proveedor);
        void Actualizar(Proveedor proveedor);
        void Eliminar(string codigo);
        IList<CatalogoProveedor> ObtenerCatalogo(string codigoProveedor);
        void AgregarProductoCatalogo(string codigoProveedor, string codigoProducto, int cantidad, decimal precio);
        void ActualizarProductoCatalogo(string codigoProveedor, string codigoProducto, int cantidad, decimal precio);
        void EliminarProductoCatalogo(string codigoProveedor, string codigoProducto);
    }

    public interface ICategoriaDatos
    {
        IList<Categoria> Listar();
        Categoria ObtenerPorCodigo(string codigo);
        void Crear(Categoria categoria);
        void Actualizar(Categoria categoria);
        void Eliminar(string codigo);
    }

    public interface IReportesDatos
    {
        DataTable Generar(TipoReporte tipo, DateTime? desde, DateTime? hasta);
    }
}