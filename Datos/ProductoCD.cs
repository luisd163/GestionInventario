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
            const string sql = @"SELECT [PrCódigo], [PrNombre], [PrDescripción], [Categoría]
FROM [Producto]
ORDER BY [PrCódigo];";
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
            const string sql = @"SELECT [PrCódigo], [PrNombre], [PrDescripción], [Categoría]
FROM [Producto]
WHERE [PrCódigo] = @PrCodigo;";
            DataTable tabla = SqlDatosHelper.Consultar(conexionBD, sql, comando => SqlDatosHelper.AgregarChar(comando, "@PrCodigo", 3, codigo));
            return tabla.Rows.Count == 0 ? null : Mapear(tabla.Rows[0]);
        }

        public void Insertar(Producto producto)
        {
            Validar(producto);
            const string sql = @"INSERT INTO [Producto] ([PrCódigo], [PrNombre], [PrDescripción], [Categoría])
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
SET [PrNombre] = @PrNombre, [PrDescripción] = @PrDescripcion, [Categoría] = @Categoria
WHERE [PrCódigo] = @PrCodigo;";
            int filas = SqlDatosHelper.Ejecutar(conexionBD, sql, comando => AgregarParametros(comando, producto));
            ExigirFilaAfectada(filas, producto.PrCodigo);
        }

        public void Eliminar(string codigo)
        {
            const string sql = "DELETE FROM [Producto] WHERE [PrCódigo] = @PrCodigo;";
            int filas = SqlDatosHelper.Ejecutar(conexionBD, sql, comando => SqlDatosHelper.AgregarChar(comando, "@PrCodigo", 3, codigo));
            ExigirFilaAfectada(filas, codigo);
        }

        private static Producto Mapear(DataRow fila)
        {
            return new Producto(
                Convert.ToString(fila["PrCódigo"]),
                Convert.ToString(fila["PrNombre"]),
                Convert.ToString(fila["PrDescripción"]),
                Convert.ToString(fila["Categoría"]));
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
