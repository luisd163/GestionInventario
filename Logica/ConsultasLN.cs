using System;
using System.Collections.Generic;
using Modelo;

namespace Logica
{
    public sealed class ConsultasLN
    {
        private readonly IConsultasDatos consultasDatos;

        public ConsultasLN(IConsultasDatos consultasDatos)
        {
            this.consultasDatos = consultasDatos ?? throw new ArgumentNullException(nameof(consultasDatos));
        }

        public IList<OfertaProductoDTO> CatalogoUnificadoOferta()
        {
            ReglasLN.ExigirSesion();
            return consultasDatos.CatalogoUnificadoOferta();
        }

        // Consulta de auditoría: solo administrador.
        public IList<AuditoriaAccesoDTO> AuditoriaAccesosPorUsuario()
        {
            ReglasLN.ExigirAdministrador();
            return consultasDatos.AuditoriaAccesosPorUsuario();
        }

        public IList<DistribucionProveedorDTO> DistribucionProveedoresFamilia()
        {
            ReglasLN.ExigirSesion();
            return consultasDatos.DistribucionProveedoresFamilia();
        }

        public IList<ProveedorPrecioSobrePromDTO> ProveedoresPrecioSobrePromedio()
        {
            ReglasLN.ExigirSesion();
            return consultasDatos.ProveedoresPrecioSobrePromedio();
        }

        public IList<ProductoSinOfertaDTO> ProductosSinOferta()
        {
            ReglasLN.ExigirSesion();
            return consultasDatos.ProductosSinOferta();
        }
    }
}