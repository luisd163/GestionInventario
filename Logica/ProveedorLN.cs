using System;
using System.Collections.Generic;
using Modelo;

namespace Logica
{
    public sealed class ProveedorLN
    {
        private readonly IProveedorDatos proveedorDatos;

        public ProveedorLN(IProveedorDatos proveedorDatos)
        {
            this.proveedorDatos = proveedorDatos ?? throw new ArgumentNullException(nameof(proveedorDatos));
        }

        public IList<Proveedor> Listar()
        {
            ReglasLN.ExigirSesion();
            return proveedorDatos.Listar();
        }

        public Proveedor ObtenerPorCodigo(string codigo)
        {
            ReglasLN.ExigirSesion();
            ReglasLN.ValidarIdentificador(codigo, nameof(codigo));
            return proveedorDatos.ObtenerPorCodigo(codigo);
        }

        public void Crear(string codigo, string nombre, string ciudad, string direccion)
        {
            ReglasLN.ExigirEscrituraCatalogos();
            ValidarProveedor(codigo, nombre, ciudad, direccion);
            proveedorDatos.Crear(new Proveedor(codigo, nombre, ciudad, direccion));
        }

        public void Actualizar(string codigo, string nombre, string ciudad, string direccion)
        {
            ReglasLN.ExigirEscrituraCatalogos();
            ValidarProveedor(codigo, nombre, ciudad, direccion);
            proveedorDatos.Actualizar(new Proveedor(codigo, nombre, ciudad, direccion));
        }

        public void Eliminar(string codigo)
        {
            ReglasLN.ExigirEscrituraCatalogos();
            ReglasLN.ValidarIdentificador(codigo, nameof(codigo));
            proveedorDatos.Eliminar(codigo);
        }

        public IList<CatalogoProveedor> ObtenerCatalogo(string codigoProveedor)
        {
            ReglasLN.ExigirSesion();
            ReglasLN.ValidarIdentificador(codigoProveedor, nameof(codigoProveedor));
            return proveedorDatos.ObtenerCatalogo(codigoProveedor);
        }

        public void AgregarProductoCatalogo(string codigoProveedor, string codigoProducto, int cantidad, decimal precio)
        {
            ReglasLN.ExigirEscrituraCatalogos();
            ValidarCatalogo(codigoProveedor, codigoProducto, cantidad, precio);
            proveedorDatos.AgregarProductoCatalogo(codigoProveedor, codigoProducto, cantidad, precio);
        }

        public void ActualizarProductoCatalogo(string codigoProveedor, string codigoProducto, int cantidad, decimal precio)
        {
            ReglasLN.ExigirEscrituraCatalogos();
            ValidarCatalogo(codigoProveedor, codigoProducto, cantidad, precio);
            proveedorDatos.ActualizarProductoCatalogo(codigoProveedor, codigoProducto, cantidad, precio);
        }

        public void EliminarProductoCatalogo(string codigoProveedor, string codigoProducto)
        {
            ReglasLN.ExigirEscrituraCatalogos();
            ReglasLN.ValidarIdentificador(codigoProveedor, nameof(codigoProveedor));
            ReglasLN.ValidarIdentificador(codigoProducto, nameof(codigoProducto));
            proveedorDatos.EliminarProductoCatalogo(codigoProveedor, codigoProducto);
        }

        private static void ValidarProveedor(string codigo, string nombre, string ciudad, string direccion)
        {
            ReglasLN.ValidarIdentificador(codigo, nameof(codigo));
            ReglasLN.ValidarObligatorio(nombre, nameof(nombre));
            ReglasLN.ValidarObligatorio(ciudad, nameof(ciudad));
            ReglasLN.ValidarObligatorio(direccion, nameof(direccion));
        }

        private static void ValidarCatalogo(string codigoProveedor, string codigoProducto, int cantidad, decimal precio)
        {
            ReglasLN.ValidarIdentificador(codigoProveedor, nameof(codigoProveedor));
            ReglasLN.ValidarIdentificador(codigoProducto, nameof(codigoProducto));
            ReglasLN.ValidarPositivo(cantidad, nameof(cantidad));
            ReglasLN.ValidarPositivo(precio, nameof(precio));
        }
    }
}