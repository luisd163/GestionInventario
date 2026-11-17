using System;
using System.Collections.Generic;
using System.Data;
using Modelo;

namespace Logica
{
    public sealed class ReportesFiltradosLN
    {
        private readonly IReportesFiltradosDatos reportesDatos;

        public ReportesFiltradosLN(IReportesFiltradosDatos reportesDatos)
        {
            this.reportesDatos = reportesDatos ?? throw new ArgumentNullException(nameof(reportesDatos));
        }

        public IList<string> CiudadesDisponibles()
        {
            ReglasLN.ExigirSesion();
            return reportesDatos.CiudadesDisponibles();
        }

        public DataTable ProveedoresPorCiudad(string ciudad)
        {
            ReglasLN.ExigirSesion();
            ReglasLN.ValidarObligatorio(ciudad, nameof(ciudad));
            return reportesDatos.ProveedoresPorCiudad(ciudad.Trim());
        }

        public DataTable CoberturaPorProveedor(string pCodigo)
        {
            ReglasLN.ExigirSesion();
            ReglasLN.ValidarIdentificador(pCodigo, nameof(pCodigo));
            return reportesDatos.CoberturaPorProveedor(pCodigo);
        }

        public DataTable ComparativaPrecios(string prCodigo)
        {
            ReglasLN.ExigirSesion();
            ReglasLN.ValidarIdentificador(prCodigo, nameof(prCodigo));
            return reportesDatos.ComparativaPrecios(prCodigo);
        }

        public DataTable CatalogoPorCategoria(string cCodigo)
        {
            ReglasLN.ExigirSesion();
            ReglasLN.ValidarIdentificador(cCodigo, nameof(cCodigo));
            return reportesDatos.CatalogoPorCategoria(cCodigo);
        }

        public IList<ResumenCategoriaDTO> MultiCategoria(IList<string> codigosCategoria)
        {
            ReglasLN.ExigirSesion();
            if (codigosCategoria == null || codigosCategoria.Count == 0)
            {
                throw new ArgumentException("Debe seleccionar al menos una categoría.", nameof(codigosCategoria));
            }

            foreach (string codigo in codigosCategoria)
            {
                ReglasLN.ValidarIdentificador(codigo, nameof(codigosCategoria));
            }

            return reportesDatos.MultiCategoria(codigosCategoria);
        }

        public void ExportarPdf(DataTable datos, string titulo, string rutaArchivo)
        {
            ReglasLN.ExigirSesion();
            if (datos == null)
            {
                throw new ArgumentNullException(nameof(datos));
            }

            ReglasLN.ValidarObligatorio(titulo, nameof(titulo));
            ReglasLN.ValidarObligatorio(rutaArchivo, nameof(rutaArchivo));
            ExportadorPdfLN.Exportar(datos, titulo, rutaArchivo);
        }
    }
}