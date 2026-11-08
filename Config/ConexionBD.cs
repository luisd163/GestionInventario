using System;
using System.Data.SqlClient;

namespace SistemaProveedores.Config
{
	public sealed class ConexionBD
	{
		private readonly string cadenaConexion;

		public ConexionBD(string cadenaConexion)
		{
			if (string.IsNullOrWhiteSpace(cadenaConexion))
			{
				throw new ArgumentException("La cadena de conexión es obligatoria.", nameof(cadenaConexion));
			}

			this.cadenaConexion = cadenaConexion;
		}

		public SqlConnection CrearConexion()
		{
			return new SqlConnection(cadenaConexion);
		}
	}
}
