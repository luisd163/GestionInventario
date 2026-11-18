using System;
using System.Collections.Generic;
using System.Data;
using Logica;
using Modelo;
using SistemaProveedores.Config;

namespace SistemaProveedores.Datos
{
    public sealed class ClasificacionProductoCD : IClasificacionDatos
    {
        internal const string Tabla = "[ClasificacionProducto]";

        private readonly ConexionBD conexionBD;

        public ClasificacionProductoCD(ConexionBD conexionBD)
        {
            this.conexionBD = conexionBD ?? throw new ArgumentNullException(nameof(conexionBD));
        }

        public IList<ClasificacionProducto> ListarPorProducto(string prCodigo)
        {
            const string sql = "SELECT [Producto], [CategoriaProducto] FROM " + Tabla +
                               " WHERE [Producto] = @Producto ORDER BY [CategoriaProducto];";
            DataTable tabla = SqlDatosHelper.Consultar(conexionBD, sql,
                comando => SqlDatosHelper.AgregarChar(comando, "@Producto", 3, prCodigo));
            var lista = new List<ClasificacionProducto>();
            foreach (DataRow fila in tabla.Rows)
            {
                lista.Add(new ClasificacionProducto(
                    Convert.ToString(fila["Producto"]),
                    Convert.ToString(fila["CategoriaProducto"])));
            }

            return lista;
        }

        public void Asignar(string prCodigo, string cCodigo)
        {
            const string sql = "INSERT INTO " + Tabla + " ([Producto], [CategoriaProducto]) VALUES (@Producto, @Categoria);";
            SqlDatosHelper.Ejecutar(conexionBD, sql, comando =>
            {
                SqlDatosHelper.AgregarChar(comando, "@Producto", 3, prCodigo);
                SqlDatosHelper.AgregarChar(comando, "@Categoria", 3, cCodigo);
            });
        }

        public void Quitar(string prCodigo, string cCodigo)
        {
            const string sql = "DELETE FROM " + Tabla + " WHERE [Producto] = @Producto AND [CategoriaProducto] = @Categoria;";
            int filas = SqlDatosHelper.Ejecutar(conexionBD, sql, comando =>
            {
                SqlDatosHelper.AgregarChar(comando, "@Producto", 3, prCodigo);
                SqlDatosHelper.AgregarChar(comando, "@Categoria", 3, cCodigo);
            });
            if (filas == 0)
            {
                throw new InvalidOperationException("No se encontró la clasificación " + prCodigo + "/" + cCodigo + ".");
            }
        }
    }
}