using System;
using System.Collections.Generic;
using Logica;
using Modelo;

namespace Controladores
{
    public sealed class ProductoController
    {
        private readonly ProductoLN productoLN;

        public ProductoController(ProductoLN productoLN)
        {
            this.productoLN = productoLN ?? throw new ArgumentNullException(nameof(productoLN));
        }

        public IList<Producto> Listar()
        {
            return productoLN.Listar();
        }

        public Producto ObtenerPorCodigo(string prCodigo)
        {
            ValidacionLongitud.Exacta(prCodigo, 3, nameof(prCodigo));
            return productoLN.ObtenerPorCodigo(prCodigo.Trim());
        }

        public void Crear(string prCodigo, string prNombre, string prDescripcion, string codigoCategoria)
        {
            Validar(prCodigo, prNombre, prDescripcion, codigoCategoria);
            productoLN.Crear(prCodigo.Trim(), prNombre.Trim(), prDescripcion.Trim(), codigoCategoria.Trim());
        }

        public void Actualizar(string prCodigo, string prNombre, string prDescripcion, string codigoCategoria)
        {
            Validar(prCodigo, prNombre, prDescripcion, codigoCategoria);
            productoLN.Actualizar(prCodigo.Trim(), prNombre.Trim(), prDescripcion.Trim(), codigoCategoria.Trim());
        }

        public void Eliminar(string prCodigo)
        {
            ValidacionLongitud.Exacta(prCodigo, 3, nameof(prCodigo));
            productoLN.Eliminar(prCodigo.Trim());
        }

        private static void Validar(string prCodigo, string prNombre, string prDescripcion, string codigoCategoria)
        {
            ValidacionLongitud.Exacta(prCodigo, 3, nameof(prCodigo));
            ValidacionLongitud.Maxima(prNombre, 50, nameof(prNombre));
            ValidacionLongitud.Maxima(prDescripcion, 500, nameof(prDescripcion));
            ValidacionLongitud.Exacta(codigoCategoria, 3, nameof(codigoCategoria));
        }
    }
}