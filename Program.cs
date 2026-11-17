using System;
using System.Windows.Forms;
using SistemaProveedores.Config;
using Vistas;

internal static class Program
{
    private const string CadenaConexion =
        "Server=localhost\\SQLEXPRESS;" +
        "Database=BDGestionInventario;" +
        "Integrated Security=True;" +
        "TrustServerCertificate=True;";

    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        ContenedorDependencias.Inicializar(CadenaConexion);
        Application.Run(new FrmLogin());
    }
}
