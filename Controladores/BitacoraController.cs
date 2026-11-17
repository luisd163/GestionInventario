using System;
using System.Data;
using Logica;

namespace Controladores
{
    public sealed class BitacoraController
    {
        private readonly ReportesLN reportesLN;

        public BitacoraController(ReportesLN reportesLN)
        {
            this.reportesLN = reportesLN ?? throw new ArgumentNullException(nameof(reportesLN));
        }

        // Solo administrador.
        public DataTable Listar(DateTime? desde, DateTime? hasta)
        {
            ReglasLN.ExigirAdministrador();
            return reportesLN.Generar(TipoReporte.Bitacora, desde, hasta);
        }
    }
}