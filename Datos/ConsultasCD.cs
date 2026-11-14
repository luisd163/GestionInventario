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
            const string sql = @"SELECT p.[PrCodigo], p.[PrNombre], c.[CNombre], pr.[PCodigo], pr.[PNombre], cp.[CaCantidad], cp.[CaPrecio]
FROM [Producto] AS p
INNER JOIN [Categoria Producto] AS c ON c.[CCodigo] = p.[Categoria]
INNER JOIN [Catálogo Proveedor] AS cp ON cp.[Producto] = p.[PrCodigo]
INNER JOIN [Proveedor] AS pr ON pr.[PCodigo] = cp.[Proveedor]
ORDER BY p.[PrNombre], pr.[PNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql);
        }

        public DataTable InventarioPorCategoria()
        {
            const string sql = @"SELECT c.[CCodigo], c.[CNombre], p.[PrCodigo], p.[PrNombre], cp.[CaCantidad], cp.[CaPrecio]
FROM [Categoria Producto] AS c
INNER JOIN [Producto] AS p ON p.[Categoria] = c.[CCodigo]
INNER JOIN [Catálogo Proveedor] AS cp ON cp.[Producto] = p.[PrCodigo]
ORDER BY c.[CNombre], p.[PrNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql);
        }

        public DataTable ProveedoresPorCategoria()
        {
            const string sql = @"SELECT pr.[PCodigo], pr.[PNombre], c.[CNombre], p.[PrCodigo], p.[PrNombre], cp.[CaCantidad]
FROM [Proveedor] AS pr
INNER JOIN [Catálogo Proveedor] AS cp ON cp.[Proveedor] = pr.[PCodigo]
INNER JOIN [Producto] AS p ON p.[PrCodigo] = cp.[Producto]
INNER JOIN [Categoria Producto] AS c ON c.[CCodigo] = p.[Categoria]
ORDER BY pr.[PNombre], c.[CNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql);
        }

        public DataTable ProductosConExistenciaMayorA(int cantidadMinima)
        {
            const string sql = @"SELECT p.[PrCodigo], p.[PrNombre]
FROM [Producto] AS p
WHERE EXISTS
(
    SELECT 1
    FROM [Catálogo Proveedor] AS cp
    WHERE cp.[Producto] = p.[PrCodigo] AND cp.[CaCantidad] > @CantidadMinima
)
ORDER BY p.[PrNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql, comando => comando.Parameters.Add("@CantidadMinima", SqlDbType.Int).Value = cantidadMinima);
        }

        public DataTable CategoriasConProductos()
        {
            const string sql = @"SELECT c.[CCodigo], c.[CNombre]
FROM [Categoria Producto] AS c
WHERE c.[CCodigo] IN
(
    SELECT p.[Categoria]
    FROM [Producto] AS p
    WHERE p.[Categoria] IS NOT NULL
)
ORDER BY c.[CNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql);
        }
    }
}
