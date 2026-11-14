using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Logica;
using Modelo;
using SistemaProveedores.Config;

namespace SistemaProveedores.Datos
{
    public sealed class ProductoCD : IProductoDatos
    {
        private readonly ConexionBD conexionBD;

        public ProductoCD(ConexionBD conexionBD)
        {
            this.conexionBD = conexionBD ?? throw new ArgumentNullException(nameof(conexionBD));
        }

        public IList<Producto> Listar()
        {
            const string sql = @"SELECT [PrCodigo], [PrNombre], [PrDescripcion], [Categoria]
FROM [Producto]
ORDER BY [PrCodigo];";
            DataTable tabla = SqlDatosHelper.Consultar(conexionBD, sql);
            var productos = new List<Producto>();
            foreach (DataRow fila in tabla.Rows)
            {
                productos.Add(Mapear(fila));
            }

            return productos;
        }

        public Producto ObtenerPorCodigo(string codigo)
        {
            const string sql = @"SELECT [PrCodigo], [PrNombre], [PrDescripcion], [Categoria]
FROM [Producto]
WHERE [PrCodigo] = @PrCodigo;";
            DataTable tabla = SqlDatosHelper.Consultar(conexionBD, sql, comando => SqlDatosHelper.AgregarChar(comando, "@PrCodigo", 3, codigo));
            return tabla.Rows.Count == 0 ? null : Mapear(tabla.Rows[0]);
        }

        public void Insertar(Producto producto)
        {
            Validar(producto);
            const string sql = @"INSERT INTO [Producto] ([PrCodigo], [PrNombre], [PrDescripcion], [Categoria])
VALUES (@PrCodigo, @PrNombre, @PrDescripcion, @Categoria);";
            SqlDatosHelper.Ejecutar(conexionBD, sql, comando => AgregarParametros(comando, producto));
        }

        public void Crear(Producto producto)
        {
            Insertar(producto);
        }

        public void Actualizar(Producto producto)
        {
            Validar(producto);
            const string sql = @"UPDATE [Producto]
SET [PrNombre] = @PrNombre, [PrDescripcion] = @PrDescripcion, [Categoria] = @Categoria
WHERE [PrCodigo] = @PrCodigo;";
            int filas = SqlDatosHelper.Ejecutar(conexionBD, sql, comando => AgregarParametros(comando, producto));
            ExigirFilaAfectada(filas, producto.PrCodigo);
        }

        public void Eliminar(string codigo)
        {
            const string sql = "DELETE FROM [Producto] WHERE [PrCodigo] = @PrCodigo;";
            int filas = SqlDatosHelper.Ejecutar(conexionBD, sql, comando => SqlDatosHelper.AgregarChar(comando, "@PrCodigo", 3, codigo));
            ExigirFilaAfectada(filas, codigo);
        }

        private static Producto Mapear(DataRow fila)
        {
            return new Producto(
                Convert.ToString(fila["PrCodigo"]),
                Convert.ToString(fila["PrNombre"]),
                Convert.ToString(fila["PrDescripcion"]),
                Convert.ToString(fila["Categoria"]));
        }

        private static void AgregarParametros(SqlCommand comando, Producto producto)
        {
            SqlDatosHelper.AgregarChar(comando, "@PrCodigo", 3, producto.PrCodigo);
            SqlDatosHelper.AgregarVarChar(comando, "@PrNombre", 50, producto.PrNombre);
            SqlDatosHelper.AgregarVarChar(comando, "@PrDescripcion", 500, producto.PrDescripcion);
            SqlDatosHelper.AgregarChar(comando, "@Categoria", 3, producto.CategoriaProducto);
        }

        private static void Validar(Producto producto)
        {
            if (producto == null)
            {
                throw new ArgumentNullException(nameof(producto));
            }
        }

        private static void ExigirFilaAfectada(int filas, string codigo)
        {
            if (filas == 0)
            {
                throw new InvalidOperationException("No se encontró el producto " + codigo + ".");
            }
        }
    }
}
