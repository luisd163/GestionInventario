using System.Data.SqlClient;
using SistemaProveedores.Config;
using SistemaProveedores.Datos;
using Logica;

class Program
{
    static void Main()
    {
        string cadenaConexion =
            "Server=localhost\\SQLEXPRESS;" +
            "Database=BDGestionInventario;" +
            "Integrated Security=True;" +
            "TrustServerCertificate=True;";

        var conexionBD = new ConexionBD(cadenaConexion);

         try
        {
            using (SqlConnection conexion = conexionBD.CrearConexion())
            using (SqlCommand comando = new SqlCommand("SELECT DB_NAME()", conexion))
            {
                conexion.Open();

                string baseDatos = Convert.ToString(comando.ExecuteScalar());

                Console.WriteLine("Conexión exitosa.");
                Console.WriteLine("Base de datos: " + baseDatos);
            }
        }
        catch (SqlException error)
        {
            Console.WriteLine("Error de conexión:");
            Console.WriteLine(error.Message);
        }

        var usuarioCD = new UsuarioCD(conexionBD);
        var bitacoraCD = new BitacoraCD(conexionBD);
        var productoCD = new ProductoCD(conexionBD);
        var proveedorCD = new ProveedorCD(conexionBD);
        var categoriaCD = new CategoriaCD(conexionBD);
        var reportesCD = new ReportesCD(conexionBD);

        var authLN = new AuthLN(usuarioCD, bitacoraCD);
        var usuarioLN = new UsuarioLN(usuarioCD);
        var productoLN = new ProductoLN(productoCD);
        var proveedorLN = new ProveedorLN(proveedorCD);
        var categoriaLN = new CategoriaLN(categoriaCD);
        var reportesLN = new ReportesLN(reportesCD);

        try
        {
            var productos = productoCD.Listar();
            Console.WriteLine("Consulta ejecutada correctamente.");
            Console.WriteLine("Productos encontrados: " + productos.Count);
            foreach (var producto in productos)
            {
                Console.WriteLine("- " + producto.PrNombre);
            }
        }
        catch (SqlException error)
        {
            Console.WriteLine("La conexión funciona, pero la consulta falló:");
            Console.WriteLine(error.Message);
        }

        // Aquí inicia la interfaz de la aplicación
    }
}