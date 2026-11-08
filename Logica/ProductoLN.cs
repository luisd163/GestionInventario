using System;
using System.Collections.Generic;
using Modelo;

namespace Logica
{
    public sealed class ProductoLN
    {
        private readonly IProductoDatos productoDatos;

        public ProductoLN(IProductoDatos productoDatos)
        {
            this.productoDatos = productoDatos ?? throw new ArgumentNullException(nameof(productoDatos));
        }

        public IList<Producto> Listar()
        {
            ReglasLN.ExigirSesion();
            return productoDatos.Listar();
        }

        public Producto ObtenerPorCodigo(string codigo)
        {
            ReglasLN.ExigirSesion();
            ReglasLN.ValidarIdentificador(codigo, nameof(codigo));
            return productoDatos.ObtenerPorCodigo(codigo);
        }

        public void Crear(string codigo, string nombre, string descripcion, string codigoCategoria)
        {
            ReglasLN.ExigirEscrituraCatalogos();
            ValidarProducto(codigo, nombre, descripcion, codigoCategoria);
            productoDatos.Crear(new Producto(codigo, nombre, descripcion, codigoCategoria));
        }

        public void Actualizar(string codigo, string nombre, string descripcion, string codigoCategoria)
        {
            ReglasLN.ExigirEscrituraCatalogos();
            ValidarProducto(codigo, nombre, descripcion, codigoCategoria);
            productoDatos.Actualizar(new Producto(codigo, nombre, descripcion, codigoCategoria));
        }

        public void Eliminar(string codigo)
        {
            ReglasLN.ExigirEscrituraCatalogos();
            ReglasLN.ValidarIdentificador(codigo, nameof(codigo));
            productoDatos.Eliminar(codigo);
        }

        private static void ValidarProducto(string codigo, string nombre, string descripcion, string codigoCategoria)
        {
            ReglasLN.ValidarIdentificador(codigo, nameof(codigo));
            ReglasLN.ValidarObligatorio(nombre, nameof(nombre));
            ReglasLN.ValidarObligatorio(descripcion, nameof(descripcion));
            ReglasLN.ValidarIdentificador(codigoCategoria, nameof(codigoCategoria));
        }
    }
}