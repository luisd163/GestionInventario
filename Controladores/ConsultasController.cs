using System;
using System.Collections.Generic;
using Logica;
using Modelo;

namespace Controladores
{
    public sealed class ConsultasController
    {
        private readonly ConsultasLN consultasLN;

        public ConsultasController(ConsultasLN consultasLN)
        {
            this.consultasLN = consultasLN ?? throw new ArgumentNullException(nameof(consultasLN));
        }

        public IList<OfertaProductoDTO> CatalogoUnificadoOferta()
        {
            return consultasLN.CatalogoUnificadoOferta();
        }

        public IList<AuditoriaAccesoDTO> AuditoriaAccesosPorUsuario()
        {
            return consultasLN.AuditoriaAccesosPorUsuario();
        }

        public IList<DistribucionProveedorDTO> DistribucionProveedoresFamilia()
        {
            return consultasLN.DistribucionProveedoresFamilia();
        }

        public IList<ProveedorPrecioSobrePromDTO> ProveedoresPrecioSobrePromedio()
        {
            return consultasLN.ProveedoresPrecioSobrePromedio();
        }

        public IList<ProductoSinOfertaDTO> ProductosSinOferta()
        {
            return consultasLN.ProductosSinOferta();
        }
    }
}