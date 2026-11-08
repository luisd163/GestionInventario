using System;
using System.Data;
using System.Data.SqlClient;
using Logica;
using SistemaProveedores.Config;

namespace SistemaProveedores.Datos
{
    public sealed class BitacoraCD : IBitacoraDatos
    {
        private readonly ConexionBD conexionBD;
        private const string AlfabetoCodigo = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public BitacoraCD(ConexionBD conexionBD)
        {
            this.conexionBD = conexionBD ?? throw new ArgumentNullException(nameof(conexionBD));
        }

        public string RegistrarIngreso(string uCodigo)
        {
            return RegistrarIngresoInterno(uCodigo, null);
        }

        public string RegistrarEntrada(string codigoUsuario, DateTime fechaHoraEntrada)
        {
            return RegistrarIngresoInterno(codigoUsuario, fechaHoraEntrada);
        }

        public void RegistrarSalida(string bCodigo)
        {
            const string sql = "UPDATE [Bitácora] SET [BTiempoSalida] = GETDATE() WHERE [BCódigo] = @BCodigo;";
            EjecutarSalida(sql, bCodigo, null);
        }

        public void RegistrarSalida(string codigoBitacora, DateTime fechaHoraSalida)
        {
            const string sql = "UPDATE [Bitácora] SET [BTiempoSalida] = @BTiempoSalida WHERE [BCódigo] = @BCodigo;";
            EjecutarSalida(sql, codigoBitacora, fechaHoraSalida);
        }

        public DataTable ListarBitacora()
        {
            const string sql = @"SELECT b.[BCódigo], b.[BTiempoIngreso], b.[BTiempoSalida], b.[Usuario], u.[UNombre]
FROM [Bitácora] AS b
INNER JOIN [Usuario] AS u ON u.[UCódigo] = b.[Usuario]
ORDER BY b.[BTiempoIngreso] DESC;";
            return SqlDatosHelper.Consultar(conexionBD, sql);
        }

        private string RegistrarIngresoInterno(string uCodigo, DateTime? fechaHoraEntrada)
        {
            using (SqlConnection conexion = conexionBD.CrearConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        string ultimoCodigo;
                        const string sqlUltimo = "SELECT MAX([BCódigo]) FROM [Bitácora] WITH (UPDLOCK, HOLDLOCK);";
                        using (SqlCommand consulta = new SqlCommand(sqlUltimo, conexion, transaccion))
                        {
                            object resultado = consulta.ExecuteScalar();
                            ultimoCodigo = resultado == DBNull.Value ? null : Convert.ToString(resultado).Trim();
                        }

                        string nuevoCodigo = SiguienteCodigo(ultimoCodigo);
                        string sqlInsertar = fechaHoraEntrada.HasValue
                            ? @"INSERT INTO [Bitácora] ([BCódigo], [BTiempoIngreso], [BTiempoSalida], [Usuario])
VALUES (@BCodigo, @BTiempoIngreso, NULL, @Usuario);"
                            : @"INSERT INTO [Bitácora] ([BCódigo], [BTiempoIngreso], [BTiempoSalida], [Usuario])
VALUES (@BCodigo, GETDATE(), NULL, @Usuario);";

                        using (SqlCommand insertar = new SqlCommand(sqlInsertar, conexion, transaccion))
                        {
                            SqlDatosHelper.AgregarChar(insertar, "@BCodigo", 3, nuevoCodigo);
                            SqlDatosHelper.AgregarChar(insertar, "@Usuario", 3, uCodigo);
                            if (fechaHoraEntrada.HasValue)
                            {
                                insertar.Parameters.Add("@BTiempoIngreso", SqlDbType.DateTime).Value = fechaHoraEntrada.Value;
                            }

                            insertar.ExecuteNonQuery();
                        }

                        transaccion.Commit();
                        return nuevoCodigo;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        private void EjecutarSalida(string sql, string codigoBitacora, DateTime? fechaHoraSalida)
        {
            using (SqlConnection conexion = conexionBD.CrearConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                SqlDatosHelper.AgregarChar(comando, "@BCodigo", 3, codigoBitacora);
                if (fechaHoraSalida.HasValue)
                {
                    comando.Parameters.Add("@BTiempoSalida", SqlDbType.DateTime).Value = fechaHoraSalida.Value;
                }

                conexion.Open();
                if (comando.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException("No se encontró la bitácora " + codigoBitacora + ".");
                }
            }
        }

        private static string SiguienteCodigo(string codigoActual)
        {
            if (string.IsNullOrEmpty(codigoActual))
            {
                return "000";
            }

            char[] digitos = codigoActual.ToUpperInvariant().PadLeft(3, '0').ToCharArray();
            for (int indice = digitos.Length - 1; indice >= 0; indice--)
            {
                int valor = AlfabetoCodigo.IndexOf(digitos[indice]);
                if (valor < 0)
                {
                    throw new InvalidOperationException("El último código de bitácora no tiene formato base 36.");
                }

                if (valor < AlfabetoCodigo.Length - 1)
                {
                    digitos[indice] = AlfabetoCodigo[valor + 1];
                    return new string(digitos);
                }

                digitos[indice] = AlfabetoCodigo[0];
            }

            throw new InvalidOperationException("Se agotaron los códigos disponibles para la bitácora.");
        }
    }
}
