using System;
using System.Collections.Generic;
using System.Data;
using Logica;
using Modelo;
using SistemaProveedores.Config;

namespace SistemaProveedores.Datos
{
    public sealed class CategoriaCD : ICategoriaDatos
    {
        private readonly ConexionBD conexionBD;

        public CategoriaCD(ConexionBD conexionBD)
        {
            this.conexionBD = conexionBD ?? throw new ArgumentNullException(nameof(conexionBD));
        }

        public IList<Categoria> Listar()
        {
            const string sql = "SELECT [CCodigo], [CNombre] FROM [Categoria Producto] ORDER BY [CCodigo];";
            DataTable tabla = SqlDatosHelper.Consultar(conexionBD, sql);
            var categorias = new List<Categoria>();
            foreach (DataRow fila in tabla.Rows)
            {
                categorias.Add(Mapear(fila));
            }

            return categorias;
        }

        public Categoria ObtenerPorCodigo(string codigo)
        {
            const string sql = "SELECT [CCodigo], [CNombre] FROM [Categoria Producto] WHERE [CCodigo] = @CCodigo;";
            DataTable tabla = SqlDatosHelper.Consultar(conexionBD, sql, comando => SqlDatosHelper.AgregarChar(comando, "@CCodigo", 3, codigo));
            return tabla.Rows.Count == 0 ? null : Mapear(tabla.Rows[0]);
        }

        public void Insertar(Categoria categoria)
        {
            Validar(categoria);
            const string sql = "INSERT INTO [Categoria Producto] ([CCodigo], [CNombre]) VALUES (@CCodigo, @CNombre);";
            SqlDatosHelper.Ejecutar(conexionBD, sql, comando => AgregarParametros(comando, categoria));
        }

        public void Crear(Categoria categoria)
        {
            Insertar(categoria);
        }

        public void Actualizar(Categoria categoria)
        {
            Validar(categoria);
            const string sql = "UPDATE [Categoria Producto] SET [CNombre] = @CNombre WHERE [CCodigo] = @CCodigo;";
            int filas = SqlDatosHelper.Ejecutar(conexionBD, sql, comando => AgregarParametros(comando, categoria));
            ExigirFilaAfectada(filas, categoria.CCodigo);
        }

        public void Eliminar(string codigo)
        {
            const string sql = "DELETE FROM [Categoria Producto] WHERE [CCodigo] = @CCodigo;";
            int filas = SqlDatosHelper.Ejecutar(conexionBD, sql, comando => SqlDatosHelper.AgregarChar(comando, "@CCodigo", 3, codigo));
            ExigirFilaAfectada(filas, codigo);
        }

        private static Categoria Mapear(DataRow fila)
        {
            return new Categoria(Convert.ToString(fila["CCodigo"]), Convert.ToString(fila["CNombre"]));
        }

        private static void AgregarParametros(System.Data.SqlClient.SqlCommand comando, Categoria categoria)
        {
            SqlDatosHelper.AgregarChar(comando, "@CCodigo", 3, categoria.CCodigo);
            SqlDatosHelper.AgregarVarChar(comando, "@CNombre", 30, categoria.CNombre);
        }

        private static void Validar(Categoria categoria)
        {
            if (categoria == null)
            {
                throw new ArgumentNullException(nameof(categoria));
            }
        }

        private static void ExigirFilaAfectada(int filas, string codigo)
        {
            if (filas == 0)
            {
                throw new InvalidOperationException("No se encontró la categoría " + codigo + ".");
            }
        }
    }
}
