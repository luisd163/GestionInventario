using System;
using System.Collections.Generic;
using System.Data;
using Logica;
using Modelo;
using SistemaProveedores.Config;

namespace SistemaProveedores.Datos
{
    public sealed class RolUsuarioCD : IRolUsuarioDatos
    {
        private readonly ConexionBD conexionBD;

        public RolUsuarioCD(ConexionBD conexionBD)
        {
            this.conexionBD = conexionBD ?? throw new ArgumentNullException(nameof(conexionBD));
        }

        public IList<RolUsuario> Listar()
        {
            const string sql = "SELECT [RCodigo], [RNombre] FROM [RolUsuario] ORDER BY [RCodigo];";
            DataTable tabla = SqlDatosHelper.Consultar(conexionBD, sql);
            var roles = new List<RolUsuario>();
            foreach (DataRow fila in tabla.Rows)
            {
                roles.Add(new RolUsuario(Convert.ToString(fila["RCodigo"]), Convert.ToString(fila["RNombre"])));
            }

            return roles;
        }
    }
}