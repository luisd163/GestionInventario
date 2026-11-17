using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Logica;
using Modelo;
using SistemaProveedores.Config;

namespace SistemaProveedores.Datos
{
    public sealed class ReportesFiltradosCD : IReportesFiltradosDatos
    {
        // Categorías de un producto: FK directa Producto.Categoria UNION tabla M:N de clasificación.
        private const string Clasif =
            "(SELECT [PrCodigo] AS [Prod], [Categoria] AS [Cat] FROM [Producto] WHERE [Categoria] IS NOT NULL " +
            "UNION SELECT [Producto], [CategoriaProducto] FROM " + ClasificacionProductoCD.Tabla + ")";

        private readonly ConexionBD conexionBD;

        public ReportesFiltradosCD(ConexionBD conexionBD)
        {
            this.conexionBD = conexionBD ?? throw new ArgumentNullException(nameof(conexionBD));
        }

        public IList<string> CiudadesDisponibles()
        {
            const string sql = "SELECT DISTINCT [PCiudad] FROM [Proveedor] ORDER BY [PCiudad];";
            var ciudades = new List<string>();
            foreach (DataRow f in SqlDatosHelper.Consultar(conexionBD, sql).Rows)
            {
                ciudades.Add(Convert.ToString(f["PCiudad"]));
            }

            return ciudades;
        }

        // Reporte 1
        public DataTable ProveedoresPorCiudad(string ciudad)
        {
            const string sql = @"SELECT pr.[PCodigo], pr.[PNombre], pr.[PCiudad], pr.[PDireccion],
       p.[PrNombre], cp.[CaCantidad], cp.[CaPrecio]
FROM [Proveedor] AS pr
LEFT JOIN [Catálogo Proveedor] AS cp ON cp.[Proveedor] = pr.[PCodigo]
LEFT JOIN [Producto] AS p ON p.[PrCodigo] = cp.[Producto]
WHERE pr.[PCiudad] = @Ciudad
ORDER BY pr.[PNombre], p.[PrNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql,
                comando => SqlDatosHelper.AgregarVarChar(comando, "@Ciudad", 30, ciudad));
        }

        // Reporte 2
        public DataTable CoberturaPorProveedor(string pCodigo)
        {
            const string sql = @"SELECT pr.[PCodigo], pr.[PNombre], p.[PrCodigo], p.[PrNombre], cp.[CaCantidad], cp.[CaPrecio]
FROM [Proveedor] AS pr
INNER JOIN [Catálogo Proveedor] AS cp ON cp.[Proveedor] = pr.[PCodigo]
INNER JOIN [Producto] AS p ON p.[PrCodigo] = cp.[Producto]
WHERE pr.[PCodigo] = @Proveedor
ORDER BY p.[PrNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql,
                comando => SqlDatosHelper.AgregarChar(comando, "@Proveedor", 3, pCodigo));
        }

        // Reporte 3: menor a mayor precio
        public DataTable ComparativaPrecios(string prCodigo)
        {
            const string sql = @"SELECT p.[PrCodigo], p.[PrNombre], pr.[PCodigo], pr.[PNombre], pr.[PCiudad], cp.[CaCantidad], cp.[CaPrecio]
FROM [Producto] AS p
INNER JOIN [Catálogo Proveedor] AS cp ON cp.[Producto] = p.[PrCodigo]
INNER JOIN [Proveedor] AS pr ON pr.[PCodigo] = cp.[Proveedor]
WHERE p.[PrCodigo] = @Producto
ORDER BY cp.[CaPrecio] ASC, pr.[PNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql,
                comando => SqlDatosHelper.AgregarChar(comando, "@Producto", 3, prCodigo));
        }

        // Reporte 4
        public DataTable CatalogoPorCategoria(string cCodigo)
        {
            const string sql = @"SELECT DISTINCT c.[CCodigo], c.[CNombre], p.[PrCodigo], p.[PrNombre], p.[PrDescripcion]
FROM [Categoria Producto] AS c
INNER JOIN " + Clasif + @" AS k ON k.[Cat] = c.[CCodigo]
INNER JOIN [Producto] AS p ON p.[PrCodigo] = k.[Prod]
WHERE c.[CCodigo] = @Categoria
ORDER BY p.[PrNombre];";
            return SqlDatosHelper.Consultar(conexionBD, sql,
                comando => SqlDatosHelper.AgregarChar(comando, "@Categoria", 3, cCodigo));
        }

        // Reporte 5
        public IList<ResumenCategoriaDTO> MultiCategoria(IList<string> codigosCategoria)
        {
            if (codigosCategoria == null || codigosCategoria.Count == 0)
            {
                throw new ArgumentException("Debe seleccionar al menos una categoría.", nameof(codigosCategoria));
            }

            var parametros = new StringBuilder();
            for (int i = 0; i < codigosCategoria.Count; i++)
            {
                if (i > 0)
                {
                    parametros.Append(", ");
                }

                parametros.Append("@c").Append(i);
            }

            string sql = @"SELECT c.[CCodigo], c.[CNombre],
       COUNT(DISTINCT k.[Prod]) AS [TotalProductos],
       COUNT(DISTINCT cp.[Proveedor]) AS [TotalProveedores]
FROM [Categoria Producto] AS c
INNER JOIN " + Clasif + @" AS k ON k.[Cat] = c.[CCodigo]
LEFT JOIN [Catálogo Proveedor] AS cp ON cp.[Producto] = k.[Prod]
WHERE c.[CCodigo] IN (" + parametros + @")
GROUP BY c.[CCodigo], c.[CNombre]
ORDER BY c.[CNombre];";

            DataTable tabla = SqlDatosHelper.Consultar(conexionBD, sql, comando =>
            {
                for (int i = 0; i < codigosCategoria.Count; i++)
                {
                    SqlDatosHelper.AgregarChar(comando, "@c" + i, 3, codigosCategoria[i]);
                }
            });

            var lista = new List<ResumenCategoriaDTO>();
            foreach (DataRow f in tabla.Rows)
            {
                lista.Add(new ResumenCategoriaDTO
                {
                    CCodigo = Convert.ToString(f["CCodigo"]),
                    CNombre = Convert.ToString(f["CNombre"]),
                    TotalProductos = Convert.ToInt32(f["TotalProductos"]),
                    TotalProveedores = Convert.ToInt32(f["TotalProveedores"])
                });
            }

            return lista;
        }
    }
}