using System;
using System.Collections.Generic;
using Modelo;

namespace Logica
{
    public sealed class ClasificacionProductoLN
    {
        private readonly IClasificacionDatos clasificacionDatos;

        public ClasificacionProductoLN(IClasificacionDatos clasificacionDatos)
        {
            this.clasificacionDatos = clasificacionDatos ?? throw new ArgumentNullException(nameof(clasificacionDatos));
        }

        public IList<ClasificacionProducto> ListarPorProducto(string prCodigo)
        {
            ReglasLN.ExigirSesion();
            ReglasLN.ValidarIdentificador(prCodigo, nameof(prCodigo));
            return clasificacionDatos.ListarPorProducto(prCodigo);
        }

        public void Asignar(string prCodigo, string cCodigo)
        {
            ReglasLN.ExigirEscrituraCatalogos();
            ReglasLN.ValidarIdentificador(prCodigo, nameof(prCodigo));
            ReglasLN.ValidarIdentificador(cCodigo, nameof(cCodigo));
            clasificacionDatos.Asignar(prCodigo, cCodigo);
        }

        public void Quitar(string prCodigo, string cCodigo)
        {
            ReglasLN.ExigirEscrituraCatalogos();
            ReglasLN.ValidarIdentificador(prCodigo, nameof(prCodigo));
            ReglasLN.ValidarIdentificador(cCodigo, nameof(cCodigo));
            clasificacionDatos.Quitar(prCodigo, cCodigo);
        }
    }
}