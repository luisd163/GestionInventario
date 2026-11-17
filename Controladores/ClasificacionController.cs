using System;
using System.Collections.Generic;
using Logica;
using Modelo;

namespace Controladores
{
    public sealed class ClasificacionController
    {
        private readonly ClasificacionProductoLN clasificacionLN;

        public ClasificacionController(ClasificacionProductoLN clasificacionLN)
        {
            this.clasificacionLN = clasificacionLN ?? throw new ArgumentNullException(nameof(clasificacionLN));
        }

        public IList<ClasificacionProducto> ListarPorProducto(string prCodigo)
        {
            ValidacionLongitud.Exacta(prCodigo, 3, nameof(prCodigo));
            return clasificacionLN.ListarPorProducto(prCodigo.Trim());
        }

        public void Asignar(string prCodigo, string cCodigo)
        {
            ValidacionLongitud.Exacta(prCodigo, 3, nameof(prCodigo));
            ValidacionLongitud.Exacta(cCodigo, 3, nameof(cCodigo));
            clasificacionLN.Asignar(prCodigo.Trim(), cCodigo.Trim());
        }

        public void Quitar(string prCodigo, string cCodigo)
        {
            ValidacionLongitud.Exacta(prCodigo, 3, nameof(prCodigo));
            ValidacionLongitud.Exacta(cCodigo, 3, nameof(cCodigo));
            clasificacionLN.Quitar(prCodigo.Trim(), cCodigo.Trim());
        }
    }
}