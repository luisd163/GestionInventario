using System;
using System.Data;
using System.Data.SqlClient;
using Logica;
using SistemaProveedores.Config;

namespace SistemaProveedores.Datos
{
    public sealed class ReportesCD : IReportesDatos
    {
        private readonly ConexionBD conexionBD;

        public ReportesCD(ConexionBD conexionBD)
        {
            this.conexionBD = conexionBD ?? throw new ArgumentNullException(nameof(conexionBD));
        }

        public DataTable ProductosDeProveedor(string codigoProveedor)
        {
            const string sql = @"SELECT pr.[PCodigo], pr.[PNombre], p.[PrCodigo], p.[PrNombre], cp.[CaCantidad], cp.[CaPrecio]
FROM [Proveedor] AS pr
INNER JOIN [Catálogo Proveedor] AS cp ON cp.[Proveedor] = pr.[PCodigo]
INNER JOIN [Producto] AS p ON p.[PrCodigo] = cp.[Producto]
WHERE (@Proveedor IS NULL OR pr.[PCodigo] = @Proveedor)
ORDER BY pr.[PNombre], p.[PrNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql, comando => SqlDatosHelper.AgregarChar(comando, "@Proveedor", 3, codigoProveedor));
        }

        public DataTable ProveedoresDeProducto(string codigoProducto)
        {
            const string sql = @"SELECT p.[PrCodigo], p.[PrNombre], pr.[PCodigo], pr.[PNombre], pr.[PCiudad], cp.[CaCantidad], cp.[CaPrecio]
FROM [Producto] AS p
INNER JOIN [Catálogo Proveedor] AS cp ON cp.[Producto] = p.[PrCodigo]
INNER JOIN [Proveedor] AS pr ON pr.[PCodigo] = cp.[Proveedor]
WHERE (@Producto IS NULL OR p.[PrCodigo] = @Producto)
ORDER BY p.[PrNombre], pr.[PNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql, comando => SqlDatosHelper.AgregarChar(comando, "@Producto", 3, codigoProducto));
        }

        public DataTable ProductosProveedorPorNombre(string textoProducto, string codigoProveedor)
        {
            const string sql = @"SELECT p.[PrCodigo], p.[PrNombre], pr.[PCodigo], pr.[PNombre], cp.[CaCantidad], cp.[CaPrecio]
FROM [Producto] AS p
INNER JOIN [Catálogo Proveedor] AS cp ON cp.[Producto] = p.[PrCodigo]
INNER JOIN [Proveedor] AS pr ON pr.[PCodigo] = cp.[Proveedor]
WHERE (@Texto IS NULL OR p.[PrNombre] LIKE '%' + @Texto + '%')
  AND (@Proveedor IS NULL OR pr.[PCodigo] = @Proveedor)
ORDER BY p.[PrNombre], pr.[PNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql, comando =>
            {
                SqlDatosHelper.AgregarVarChar(comando, "@Texto", 50, textoProducto);
                SqlDatosHelper.AgregarChar(comando, "@Proveedor", 3, codigoProveedor);
            });
        }

        public DataTable ProductosPorCategoria(string codigoCategoria)
        {
            const string sql = @"SELECT c.[CCodigo], c.[CNombre], p.[PrCodigo], p.[PrNombre], p.[PrDescripcion]
FROM [Categoria Producto] AS c
INNER JOIN [Producto] AS p ON p.[Categoria] = c.[CCodigo]
WHERE (@Categoria IS NULL OR c.[CCodigo] = @Categoria)
ORDER BY c.[CNombre], p.[PrNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql, comando => SqlDatosHelper.AgregarChar(comando, "@Categoria", 3, codigoCategoria));
        }

        public DataTable CategoriasPorNombreProducto(string textoProducto)
        {
            const string sql = @"SELECT c.[CCodigo], c.[CNombre], p.[PrCodigo], p.[PrNombre]
FROM [Producto] AS p
INNER JOIN [Categoria Producto] AS c ON c.[CCodigo] = p.[Categoria]
WHERE (@Texto IS NULL OR p.[PrNombre] LIKE '%' + @Texto + '%')
ORDER BY p.[PrNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql, comando => SqlDatosHelper.AgregarVarChar(comando, "@Texto", 50, textoProducto));
        }

        public DataTable Generar(TipoReporte tipo, DateTime? desde, DateTime? hasta)
        {
            switch (tipo)
            {
                case TipoReporte.Inventario:
                    return Inventario();
                case TipoReporte.Proveedores:
                    return ProductosDeProveedor(null);
                case TipoReporte.Categorias:
                    return ProductosPorCategoria(null);
                case TipoReporte.Bitacora:
                    return Bitacora(desde, hasta);
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipo), "Tipo de reporte no soportado.");
            }
        }

        private DataTable Inventario()
        {
            const string sql = @"SELECT p.[PrCodigo], p.[PrNombre], pr.[PNombre], cp.[CaCantidad], cp.[CaPrecio]
FROM [Producto] AS p
INNER JOIN [Catálogo Proveedor] AS cp ON cp.[Producto] = p.[PrCodigo]
INNER JOIN [Proveedor] AS pr ON pr.[PCodigo] = cp.[Proveedor]
ORDER BY p.[PrNombre], pr.[PNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql);
        }

        private DataTable Bitacora(DateTime? desde, DateTime? hasta)
        {
            const string sql = @"SELECT b.[BCodigo], b.[BTiempoIngreso], b.[BTiempoSalida], b.[Usuario], u.[UNombre]
FROM [Bitácora] AS b
INNER JOIN [Usuario] AS u ON u.[UCodigo] = b.[Usuario]
WHERE (@Desde IS NULL OR b.[BTiempoIngreso] >= @Desde)
  AND (@Hasta IS NULL OR b.[BTiempoIngreso] < DATEADD(day, 1, @Hasta))
ORDER BY b.[BTiempoIngreso] DESC;";
            return SqlDatosHelper.Consultar(conexionBD, sql, comando => AgregarFechas(comando, desde, hasta));
        }

        private static void AgregarFechas(SqlCommand comando, DateTime? desde, DateTime? hasta)
        {
            comando.Parameters.Add("@Desde", SqlDbType.DateTime).Value = (object)desde ?? DBNull.Value;
            comando.Parameters.Add("@Hasta", SqlDbType.DateTime).Value = (object)hasta ?? DBNull.Value;
        }
    }
}
