using System;
using System.Data;
using System.Data.SqlClient;
using SistemaProveedores.Config;

namespace SistemaProveedores.Datos
{
    internal static class SqlDatosHelper
    {
        internal static DataTable Consultar(ConexionBD conexionBD, string sql, Action<SqlCommand> agregarParametros = null)
        {
            var tabla = new DataTable();
            using (SqlConnection conexion = conexionBD.CrearConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                if (agregarParametros != null)
                {
                    agregarParametros(comando);
                }

                conexion.Open();
                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    tabla.Load(lector);
                }
            }

            return tabla;
        }

        internal static int Ejecutar(ConexionBD conexionBD, string sql, Action<SqlCommand> agregarParametros)
        {
            using (SqlConnection conexion = conexionBD.CrearConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                if (agregarParametros != null)
                {
                    agregarParametros(comando);
                }

                conexion.Open();
                return comando.ExecuteNonQuery();
            }
        }

        internal static SqlParameter AgregarChar(SqlCommand comando, string nombre, int longitud, string valor)
        {
            var parametro = comando.Parameters.Add(nombre, SqlDbType.Char, longitud);
            parametro.Value = (object)valor ?? DBNull.Value;
            return parametro;
        }

        internal static SqlParameter AgregarVarChar(SqlCommand comando, string nombre, int longitud, string valor)
        {
            var parametro = comando.Parameters.Add(nombre, SqlDbType.VarChar, longitud);
            parametro.Value = (object)valor ?? DBNull.Value;
            return parametro;
        }
    }
}
