using System;
using System.Collections.Generic;
using System.Data;
using Logica;
using Modelo;
using SistemaProveedores.Config;

namespace SistemaProveedores.Datos
{
    public sealed class ConsultasCD : IConsultasDatos
    {
        private readonly ConexionBD conexionBD;

        public ConsultasCD(ConexionBD conexionBD)
        {
            this.conexionBD = conexionBD ?? throw new ArgumentNullException(nameof(conexionBD));
        }

        public IList<OfertaProductoDTO> CatalogoUnificadoOferta()
        {
            const string sql = @"SELECT p.[PrNombre], c.[CNombre], pr.[PNombre], cp.[CaPrecio]
FROM [Producto] AS p
INNER JOIN [Categoria Producto] AS c ON c.[CCodigo] = p.[Categoria]
INNER JOIN [Catálogo Proveedor] AS cp ON cp.[Producto] = p.[PrCodigo]
INNER JOIN [Proveedor] AS pr ON pr.[PCodigo] = cp.[Proveedor]
ORDER BY p.[PrNombre], cp.[CaPrecio];";

            var resultado = new List<OfertaProductoDTO>();
            foreach (DataRow fila in SqlDatosHelper.Consultar(conexionBD, sql).Rows)
            {
                resultado.Add(new OfertaProductoDTO
                {
                    PrNombre = Convert.ToString(fila["PrNombre"]),
                    CNombre = Convert.ToString(fila["CNombre"]),
                    PNombre = Convert.ToString(fila["PNombre"]),
                    CaPrecio = Convert.ToDecimal(fila["CaPrecio"])
                });
            }

            return resultado;
        }

        public IList<AuditoriaAccesoDTO> AuditoriaAccesosPorUsuario()
        {
            const string sql = @"SELECT u.[UNombre], r.[RNombre], b.[BTiempoIngreso], b.[BTiempoSalida]
FROM [Bitácora] AS b
INNER JOIN [Usuario] AS u ON u.[UCodigo] = b.[Usuario]
INNER JOIN [Rol Usuario] AS r ON r.[RCodigo] = u.[Rol]
ORDER BY b.[BTiempoIngreso] DESC;";

            var resultado = new List<AuditoriaAccesoDTO>();
            foreach (DataRow fila in SqlDatosHelper.Consultar(conexionBD, sql).Rows)
            {
                resultado.Add(new AuditoriaAccesoDTO
                {
                    UNombre = Convert.ToString(fila["UNombre"]),
                    RNombre = Convert.ToString(fila["RNombre"]),
                    BTiempoIngreso = Convert.ToDateTime(fila["BTiempoIngreso"]),
                    BTiempoSalida = fila["BTiempoSalida"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(fila["BTiempoSalida"])
                });
            }

            return resultado;
        }

        public IList<DistribucionProveedorDTO> DistribucionProveedoresFamilia()
        {
            const string sql = @"SELECT p.[PrNombre], c.[CNombre], pr.[PCiudad]
FROM [Producto] AS p
INNER JOIN [Categoria Producto] AS c ON c.[CCodigo] = p.[Categoria]
INNER JOIN [Catálogo Proveedor] AS cp ON cp.[Producto] = p.[PrCodigo]
INNER JOIN [Proveedor] AS pr ON pr.[PCodigo] = cp.[Proveedor]
ORDER BY c.[CNombre], p.[PrNombre], pr.[PCiudad];";

            var resultado = new List<DistribucionProveedorDTO>();
            foreach (DataRow fila in SqlDatosHelper.Consultar(conexionBD, sql).Rows)
            {
                resultado.Add(new DistribucionProveedorDTO
                {
                    PrNombre = Convert.ToString(fila["PrNombre"]),
                    CNombre = Convert.ToString(fila["CNombre"]),
                    PCiudad = Convert.ToString(fila["PCiudad"])
                });
            }

            return resultado;
        }

        public IList<ProveedorPrecioSobrePromDTO> ProveedoresPrecioSobrePromedio()
        {
            const string sql = @"SELECT pr.[PCodigo], pr.[PNombre], p.[PrCodigo], cp.[CaPrecio],
(
    SELECT AVG(cp2.[CaPrecio])
    FROM [Catálogo Proveedor] AS cp2
    WHERE cp2.[Producto] = cp.[Producto]
) AS [PrecioPromedio]
FROM [Catálogo Proveedor] AS cp
INNER JOIN [Proveedor] AS pr ON pr.[PCodigo] = cp.[Proveedor]
INNER JOIN [Producto] AS p ON p.[PrCodigo] = cp.[Producto]
WHERE cp.[CaPrecio] >
(
    SELECT AVG(cp2.[CaPrecio])
    FROM [Catálogo Proveedor] AS cp2
    WHERE cp2.[Producto] = cp.[Producto]
)
ORDER BY p.[PrNombre], cp.[CaPrecio] DESC;";

            var resultado = new List<ProveedorPrecioSobrePromDTO>();
            foreach (DataRow fila in SqlDatosHelper.Consultar(conexionBD, sql).Rows)
            {
                resultado.Add(new ProveedorPrecioSobrePromDTO
                {
                    PCodigo = Convert.ToString(fila["PCodigo"]),
                    PNombre = Convert.ToString(fila["PNombre"]),
                    PrCodigo = Convert.ToString(fila["PrCodigo"]),
                    CaPrecio = Convert.ToDecimal(fila["CaPrecio"]),
                    PrecioPromedio = Convert.ToDecimal(fila["PrecioPromedio"])
                });
            }

            return resultado;
        }

        public IList<ProductoSinOfertaDTO> ProductosSinOferta()
        {
            const string sql = @"SELECT p.[PrCodigo], p.[PrNombre], p.[PrDescripcion]
FROM [Producto] AS p
WHERE NOT EXISTS
(
    SELECT 1
    FROM [Catálogo Proveedor] AS cp
    WHERE cp.[Producto] = p.[PrCodigo]
)
ORDER BY p.[PrNombre];";

            var resultado = new List<ProductoSinOfertaDTO>();
            foreach (DataRow fila in SqlDatosHelper.Consultar(conexionBD, sql).Rows)
            {
                resultado.Add(new ProductoSinOfertaDTO
                {
                    PrCodigo = Convert.ToString(fila["PrCodigo"]),
                    PrNombre = Convert.ToString(fila["PrNombre"]),
                    PrDescripcion = Convert.ToString(fila["PrDescripcion"])
                });
            }

            return resultado;
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
