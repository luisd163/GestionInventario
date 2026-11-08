using System;
using System.Collections.Generic;
using System.Data;
using Modelo;
using SistemaProveedores.Config;

namespace SistemaProveedores.Datos
{
    public sealed class CatalogoProveedorCD
    {
        private readonly ConexionBD conexionBD;

        public CatalogoProveedorCD(ConexionBD conexionBD)
        {
            this.conexionBD = conexionBD ?? throw new ArgumentNullException(nameof(conexionBD));
        }

        public IList<CatalogoProveedor> ObtenerCatalogo(string codigoProveedor)
        {
            const string sql = @"SELECT [Proveedor], [Producto], [CaCantidad], [CaPrecio]
FROM [Catálogo Proveedor]
WHERE [Proveedor] = @Proveedor
ORDER BY [Producto];";
            DataTable tabla = SqlDatosHelper.Consultar(conexionBD, sql, comando => SqlDatosHelper.AgregarChar(comando, "@Proveedor", 3, codigoProveedor));
            var filas = new List<CatalogoProveedor>();
            foreach (DataRow fila in tabla.Rows)
            {
                filas.Add(new CatalogoProveedor(
                    Convert.ToString(fila["Proveedor"]),
                    Convert.ToString(fila["Producto"]),
                    Convert.ToInt32(fila["CaCantidad"]),
                    Convert.ToDecimal(fila["CaPrecio"])));
            }

            return filas;
        }

        public void AsociarProducto(string codigoProveedor, string codigoProducto, int cantidad, decimal precio)
        {
            ValidarInventario(cantidad, precio);
            const string sql = @"INSERT INTO [Catálogo Proveedor] ([Proveedor], [Producto], [CaCantidad], [CaPrecio])
VALUES (@Proveedor, @Producto, @CaCantidad, @CaPrecio);";
            SqlDatosHelper.Ejecutar(conexionBD, sql, comando => AgregarParametros(comando, codigoProveedor, codigoProducto, cantidad, precio));
        }

        public void ActualizarInventarioPrecio(string codigoProveedor, string codigoProducto, int cantidad, decimal precio)
        {
            ValidarInventario(cantidad, precio);
            const string sql = @"UPDATE [Catálogo Proveedor]
SET [CaCantidad] = @CaCantidad, [CaPrecio] = @CaPrecio
WHERE [Proveedor] = @Proveedor AND [Producto] = @Producto;";
            int filas = SqlDatosHelper.Ejecutar(conexionBD, sql, comando => AgregarParametros(comando, codigoProveedor, codigoProducto, cantidad, precio));
            ExigirFilaAfectada(filas, codigoProveedor, codigoProducto);
        }

        public void DesasociarProducto(string codigoProveedor, string codigoProducto)
        {
            const string sql = "DELETE FROM [Catálogo Proveedor] WHERE [Proveedor] = @Proveedor AND [Producto] = @Producto;";
            int filas = SqlDatosHelper.Ejecutar(conexionBD, sql, comando =>
            {
                SqlDatosHelper.AgregarChar(comando, "@Proveedor", 3, codigoProveedor);
                SqlDatosHelper.AgregarChar(comando, "@Producto", 3, codigoProducto);
            });
            ExigirFilaAfectada(filas, codigoProveedor, codigoProducto);
        }

        private static void AgregarParametros(System.Data.SqlClient.SqlCommand comando, string codigoProveedor, string codigoProducto, int cantidad, decimal precio)
        {
            SqlDatosHelper.AgregarChar(comando, "@Proveedor", 3, codigoProveedor);
            SqlDatosHelper.AgregarChar(comando, "@Producto", 3, codigoProducto);
            comando.Parameters.Add("@CaCantidad", SqlDbType.Int).Value = cantidad;
            comando.Parameters.Add("@CaPrecio", SqlDbType.Decimal).Value = precio;
        }

        private static void ValidarInventario(int cantidad, decimal precio)
        {
            if (cantidad <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe ser mayor que cero.");
            }

            if (precio <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(precio), "El precio debe ser mayor que cero.");
            }
        }

        private static void ExigirFilaAfectada(int filas, string codigoProveedor, string codigoProducto)
        {
            if (filas == 0)
            {
                throw new InvalidOperationException("No se encontró la asociación proveedor-producto " + codigoProveedor + "/" + codigoProducto + ".");
            }
        }
    }
}
