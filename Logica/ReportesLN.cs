using System;
using System.Data;

namespace Logica
{
    public sealed class ReportesLN
    {
        private readonly IReportesDatos reportesDatos;

        public ReportesLN(IReportesDatos reportesDatos)
        {
            this.reportesDatos = reportesDatos ?? throw new ArgumentNullException(nameof(reportesDatos));
        }

        public DataTable Generar(TipoReporte tipo, DateTime? desde = null, DateTime? hasta = null)
        {
            ReglasLN.ExigirSesion();
            ReglasLN.ValidarRangoFechas(desde, hasta);
            return reportesDatos.Generar(tipo, desde, hasta);
        }
    }
}