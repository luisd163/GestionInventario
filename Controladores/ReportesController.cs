using System;
using System.Collections.Generic;
using System.Data;
using Logica;
using Modelo;

namespace Controladores
{
    public sealed class ReportesController
    {
        private readonly ReportesFiltradosLN reportesLN;

        public ReportesController(ReportesFiltradosLN reportesLN)
        {
            this.reportesLN = reportesLN ?? throw new ArgumentNullException(nameof(reportesLN));
        }

        public IList<string> CiudadesDisponibles()
        {
            return reportesLN.CiudadesDisponibles();
        }

        public DataTable ProveedoresPorCiudad(string ciudad)
        {
            ValidacionLongitud.Maxima(ciudad, 30, nameof(ciudad));
            return reportesLN.ProveedoresPorCiudad(ciudad);
        }

        public DataTable CoberturaPorProveedor(string pCodigo)
        {
            ValidacionLongitud.Exacta(pCodigo, 3, nameof(pCodigo));
            return reportesLN.CoberturaPorProveedor(pCodigo.Trim());
        }

        public DataTable ComparativaPreciosProducto(string prCodigo)
        {
            ValidacionLongitud.Exacta(prCodigo, 3, nameof(prCodigo));
            return reportesLN.ComparativaPrecios(prCodigo.Trim());
        }

        public DataTable CatalogoPorCategoria(string cCodigo)
        {
            ValidacionLongitud.Exacta(cCodigo, 3, nameof(cCodigo));
            return reportesLN.CatalogoPorCategoria(cCodigo.Trim());
        }

        public IList<ResumenCategoriaDTO> MultiCategoriaDisponibilidad(IList<string> codigosCategoria)
        {
            if (codigosCategoria != null)
            {
                foreach (string codigo in codigosCategoria)
                {
                    ValidacionLongitud.Exacta(codigo, 3, nameof(codigosCategoria));
                }
            }

            return reportesLN.MultiCategoria(codigosCategoria);
        }

        public void ExportarPdf(DataTable datos, string titulo, string rutaArchivo)
        {
            reportesLN.ExportarPdf(datos, titulo, rutaArchivo);
        }
    }
}