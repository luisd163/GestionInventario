using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Logica;
using Modelo;
using SistemaProveedores.Config;

namespace SistemaProveedores.Datos
{
    public sealed class UsuarioCD : IAuthDatos, IUsuarioDatos
    {
        private readonly ConexionBD conexionBD;

        public UsuarioCD(ConexionBD conexionBD)
        {
            this.conexionBD = conexionBD ?? throw new ArgumentNullException(nameof(conexionBD));
        }

        public Usuario Autenticar(string uCodigo, string uContrasena)
        {
            const string sql = @"SELECT u.[UNombre], u.[UCódigo], u.[RolUsuario]
FROM [Usuario] AS u
INNER JOIN [RolUsuario] AS r ON r.[RCódigo] = u.[RolUsuario]
WHERE u.[UCódigo] = @UCodigo AND u.[UContrasena] = @UContrasena;";

            using (SqlConnection conexion = conexionBD.CrearConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                SqlDatosHelper.AgregarChar(comando, "@UCodigo", 3, uCodigo);
                SqlDatosHelper.AgregarVarChar(comando, "@UContrasena", 15, uContrasena);
                conexion.Open();
                using (SqlDataReader lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!lector.Read())
                    {
                        return null;
                    }

                    return MapearUsuario(lector);
                }
            }
        }

        public UsuarioAutenticadoDatos ValidarCredenciales(string identificador, string UContrasena)
        {
            Usuario usuario = Autenticar(identificador, UContrasena);
            if (usuario == null)
            {
                return null;
            }

            return new UsuarioAutenticadoDatos
            {
                CodigoUsuario = usuario.UCodigo,
                NombreUsuario = usuario.UNombre,
                CodigoRol = usuario.RolUsuario
            };
        }

        public IList<Usuario> Listar()
        {
            const string sql = @"SELECT [UNombre], [UCódigo], [RolUsuario]
FROM [Usuario]
ORDER BY [UCódigo];";
            DataTable tabla = SqlDatosHelper.Consultar(conexionBD, sql);
            var usuarios = new List<Usuario>();
            foreach (DataRow fila in tabla.Rows)
            {
                usuarios.Add(new Usuario(
                    Convert.ToString(fila["UNombre"]),
                    Convert.ToString(fila["UCódigo"]),
                    null,
                    Convert.ToString(fila["RolUsuario"])));
            }

            return usuarios;
        }

        public Usuario ObtenerPorCodigo(string codigo)
        {
            const string sql = @"SELECT [UNombre], [UCódigo], [RolUsuario]
FROM [Usuario]
WHERE [UCódigo] = @UCodigo;";
            using (SqlConnection conexion = conexionBD.CrearConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                SqlDatosHelper.AgregarChar(comando, "@UCodigo", 3, codigo);
                conexion.Open();
                using (SqlDataReader lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return lector.Read() ? MapearUsuario(lector) : null;
                }
            }
        }

        public void Insertar(Usuario usuario)
        {
            Crear(usuario, usuario == null ? null : usuario.UContrasena);
        }

        public void Crear(Usuario usuario, string UContrasena)
        {
            ValidarUsuario(usuario);
            const string sql = @"INSERT INTO [Usuario] ([UCódigo], [UNombre], [RolUsuario], [UContrasena])
VALUES (@UCodigo, @UNombre, @RolUsuario, @UContrasena);";
            SqlDatosHelper.Ejecutar(conexionBD, sql, comando => AgregarParametrosUsuario(comando, usuario, UContrasena));
        }

        public void Actualizar(Usuario usuario, string UContrasena)
        {
            ValidarUsuario(usuario);
            const string sql = @"UPDATE [Usuario]
SET [UNombre] = @UNombre, [RolUsuario] = @RolUsuario, [UContrasena] = @UContrasena
WHERE [UCódigo] = @UCodigo;";
            int filas = SqlDatosHelper.Ejecutar(conexionBD, sql, comando => AgregarParametrosUsuario(comando, usuario, UContrasena));
            ExigirFilaAfectada(filas, usuario.UCodigo);
        }

        public void Eliminar(string codigo)
        {
            const string sql = "DELETE FROM [Usuario] WHERE [UCódigo] = @UCodigo;";
            int filas = SqlDatosHelper.Ejecutar(conexionBD, sql, comando => SqlDatosHelper.AgregarChar(comando, "@UCodigo", 3, codigo));
            ExigirFilaAfectada(filas, codigo);
        }

        private static Usuario MapearUsuario(IDataRecord registro)
        {
            return new Usuario(
                Convert.ToString(registro["UNombre"]),
                Convert.ToString(registro["UCódigo"]),
                null,
                Convert.ToString(registro["RolUsuario"]));
        }

        private static void AgregarParametrosUsuario(SqlCommand comando, Usuario usuario, string UContrasena)
        {
            SqlDatosHelper.AgregarChar(comando, "@UCodigo", 3, usuario.UCodigo);
            SqlDatosHelper.AgregarVarChar(comando, "@UNombre", 50, usuario.UNombre);
            SqlDatosHelper.AgregarChar(comando, "@RolUsuario", 3, usuario.RolUsuario);
            SqlDatosHelper.AgregarVarChar(comando, "@UContrasena", 15, UContrasena);
        }

        private static void ValidarUsuario(Usuario usuario)
        {
            if (usuario == null)
            {
                throw new ArgumentNullException(nameof(usuario));
            }

            if (string.IsNullOrWhiteSpace(usuario.UContrasena))
            {
                throw new ArgumentException("La contraseña es obligatoria.", nameof(usuario));
            }
        }

        private static void ExigirFilaAfectada(int filas, string codigo)
        {
            if (filas == 0)
            {
                throw new InvalidOperationException("No se encontró el usuario " + codigo + ".");
            }
        }
    }
}
