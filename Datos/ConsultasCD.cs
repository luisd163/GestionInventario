using System;
using System.Data;
using SistemaProveedores.Config;

namespace SistemaProveedores.Datos
{
    public sealed class ConsultasCD
    {
        private readonly ConexionBD conexionBD;

        public ConsultasCD(ConexionBD conexionBD)
        {
            this.conexionBD = conexionBD ?? throw new ArgumentNullException(nameof(conexionBD));
        }

        public DataTable ProductosProveedoresCategorias()
        {
            const string sql = @"SELECT p.[PrCódigo], p.[PrNombre], c.[CNombre], pr.[PCódigo], pr.[PNombre], cp.[CaCantidad], cp.[CaPrecio]
FROM [Producto] AS p
INNER JOIN [Categoria Producto] AS c ON c.[CCódigo] = p.[Categoría]
INNER JOIN [Catálogo Proveedor] AS cp ON cp.[Producto] = p.[PrCódigo]
INNER JOIN [Proveedor] AS pr ON pr.[PCódigo] = cp.[Proveedor]
ORDER BY p.[PrNombre], pr.[PNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql);
        }

        public DataTable InventarioPorCategoria()
        {
            const string sql = @"SELECT c.[CCódigo], c.[CNombre], p.[PrCódigo], p.[PrNombre], cp.[CaCantidad], cp.[CaPrecio]
FROM [Categoria Producto] AS c
INNER JOIN [Producto] AS p ON p.[Categoría] = c.[CCódigo]
INNER JOIN [Catálogo Proveedor] AS cp ON cp.[Producto] = p.[PrCódigo]
ORDER BY c.[CNombre], p.[PrNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql);
        }

        public DataTable ProveedoresPorCategoria()
        {
            const string sql = @"SELECT pr.[PCódigo], pr.[PNombre], c.[CNombre], p.[PrCódigo], p.[PrNombre], cp.[CaCantidad]
FROM [Proveedor] AS pr
INNER JOIN [Catálogo Proveedor] AS cp ON cp.[Proveedor] = pr.[PCódigo]
INNER JOIN [Producto] AS p ON p.[PrCódigo] = cp.[Producto]
INNER JOIN [Categoria Producto] AS c ON c.[CCódigo] = p.[Categoría]
ORDER BY pr.[PNombre], c.[CNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql);
        }

        public DataTable ProductosConExistenciaMayorA(int cantidadMinima)
        {
            const string sql = @"SELECT p.[PrCódigo], p.[PrNombre]
FROM [Producto] AS p
WHERE EXISTS
(
    SELECT 1
    FROM [Catálogo Proveedor] AS cp
    WHERE cp.[Producto] = p.[PrCódigo] AND cp.[CaCantidad] > @CantidadMinima
)
ORDER BY p.[PrNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql, comando => comando.Parameters.Add("@CantidadMinima", SqlDbType.Int).Value = cantidadMinima);
        }

        public DataTable CategoriasConProductos()
        {
            const string sql = @"SELECT c.[CCódigo], c.[CNombre]
FROM [Categoria Producto] AS c
WHERE c.[CCódigo] IN
(
    SELECT p.[Categoría]
    FROM [Producto] AS p
    WHERE p.[Categoría] IS NOT NULL
)
ORDER BY c.[CNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql);
        }
    }
}
