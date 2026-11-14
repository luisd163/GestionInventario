using System;
using System.Collections.Generic;
using System.Data;
using Logica;
using Modelo;
using SistemaProveedores.Config;

namespace SistemaProveedores.Datos
{
    public sealed class ProveedorCD : IProveedorDatos
    {
        private readonly ConexionBD conexionBD;
        private readonly CatalogoProveedorCD catalogoProveedorCD;

        public ProveedorCD(ConexionBD conexionBD)
        {
            this.conexionBD = conexionBD ?? throw new ArgumentNullException(nameof(conexionBD));
            catalogoProveedorCD = new CatalogoProveedorCD(conexionBD);
        }

        public IList<Proveedor> Listar()
        {
            const string sql = @"SELECT [PCodigo], [PNombre], [PCiudad], [PDireccion]
FROM [Proveedor]
ORDER BY [PCodigo];";
            DataTable tabla = SqlDatosHelper.Consultar(conexionBD, sql);
            var proveedores = new List<Proveedor>();
            foreach (DataRow fila in tabla.Rows)
            {
                proveedores.Add(Mapear(fila));
            }

            return proveedores;
        }

        public Proveedor ObtenerPorCodigo(string codigo)
        {
            const string sql = @"SELECT [PCodigo], [PNombre], [PCiudad], [PDireccion]
FROM [Proveedor]
WHERE [PCodigo] = @PCodigo;";
            DataTable tabla = SqlDatosHelper.Consultar(conexionBD, sql, comando => SqlDatosHelper.AgregarChar(comando, "@PCodigo", 3, codigo));
            return tabla.Rows.Count == 0 ? null : Mapear(tabla.Rows[0]);
        }

        public void Insertar(Proveedor proveedor)
        {
            Validar(proveedor);
            const string sql = @"INSERT INTO [Proveedor] ([PCodigo], [PNombre], [PCiudad], [PDireccion])
VALUES (@PCodigo, @PNombre, @PCiudad, @PDireccion);";
            SqlDatosHelper.Ejecutar(conexionBD, sql, comando => AgregarParametros(comando, proveedor));
        }

        public void Crear(Proveedor proveedor)
        {
            Insertar(proveedor);
        }

        public void Actualizar(Proveedor proveedor)
        {
            Validar(proveedor);
            const string sql = @"UPDATE [Proveedor]
SET [PNombre] = @PNombre, [PCiudad] = @PCiudad, [PDireccion] = @PDireccion
WHERE [PCodigo] = @PCodigo;";
            int filas = SqlDatosHelper.Ejecutar(conexionBD, sql, comando => AgregarParametros(comando, proveedor));
            ExigirFilaAfectada(filas, proveedor.PCodigo);
        }

        public void Eliminar(string codigo)
        {
            const string sql = "DELETE FROM [Proveedor] WHERE [PCodigo] = @PCodigo;";
            int filas = SqlDatosHelper.Ejecutar(conexionBD, sql, comando => SqlDatosHelper.AgregarChar(comando, "@PCodigo", 3, codigo));
            ExigirFilaAfectada(filas, codigo);
        }

        public IList<CatalogoProveedor> ObtenerCatalogo(string codigoProveedor)
        {
            return catalogoProveedorCD.ObtenerCatalogo(codigoProveedor);
        }

        public void AgregarProductoCatalogo(string codigoProveedor, string codigoProducto, int cantidad, decimal precio)
        {
            catalogoProveedorCD.AsociarProducto(codigoProveedor, codigoProducto, cantidad, precio);
        }

        public void ActualizarProductoCatalogo(string codigoProveedor, string codigoProducto, int cantidad, decimal precio)
        {
            catalogoProveedorCD.ActualizarInventarioPrecio(codigoProveedor, codigoProducto, cantidad, precio);
        }

        public void EliminarProductoCatalogo(string codigoProveedor, string codigoProducto)
        {
            catalogoProveedorCD.DesasociarProducto(codigoProveedor, codigoProducto);
        }

        private static Proveedor Mapear(DataRow fila)
        {
            return new Proveedor(
                Convert.ToString(fila["PCodigo"]),
                Convert.ToString(fila["PNombre"]),
                Convert.ToString(fila["PCiudad"]),
                Convert.ToString(fila["PDireccion"]));
        }

        private static void AgregarParametros(System.Data.SqlClient.SqlCommand comando, Proveedor proveedor)
        {
            SqlDatosHelper.AgregarChar(comando, "@PCodigo", 3, proveedor.PCodigo);
            SqlDatosHelper.AgregarVarChar(comando, "@PNombre", 50, proveedor.PNombre);
            SqlDatosHelper.AgregarVarChar(comando, "@PCiudad", 30, proveedor.PCiudad);
            SqlDatosHelper.AgregarVarChar(comando, "@PDireccion", 100, proveedor.PDireccion);
        }

        private static void Validar(Proveedor proveedor)
        {
            if (proveedor == null)
            {
                throw new ArgumentNullException(nameof(proveedor));
            }
        }

        private static void ExigirFilaAfectada(int filas, string codigo)
        {
            if (filas == 0)
            {
                throw new InvalidOperationException("No se encontró el proveedor " + codigo + ".");
            }
        }
    }
}
