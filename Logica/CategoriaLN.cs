using System;
using System.Collections.Generic;
using Modelo;

namespace Logica
{
    public sealed class CategoriaLN
    {
        private readonly ICategoriaDatos categoriaDatos;

        public CategoriaLN(ICategoriaDatos categoriaDatos)
        {
            this.categoriaDatos = categoriaDatos ?? throw new ArgumentNullException(nameof(categoriaDatos));
        }

        public IList<Categoria> Listar()
        {
            ReglasLN.ExigirSesion();
            return categoriaDatos.Listar();
        }

        public Categoria ObtenerPorCodigo(string codigo)
        {
            ReglasLN.ExigirSesion();
            ReglasLN.ValidarIdentificador(codigo, nameof(codigo));
            return categoriaDatos.ObtenerPorCodigo(codigo);
        }

        public void Crear(string codigo, string nombre)
        {
            ReglasLN.ExigirEscrituraCatalogos();
            ValidarCategoria(codigo, nombre);
            categoriaDatos.Crear(new Categoria(codigo, nombre));
        }

        public void Actualizar(string codigo, string nombre)
        {
            ReglasLN.ExigirEscrituraCatalogos();
            ValidarCategoria(codigo, nombre);
            categoriaDatos.Actualizar(new Categoria(codigo, nombre));
        }

        public void Eliminar(string codigo)
        {
            ReglasLN.ExigirEscrituraCatalogos();
            ReglasLN.ValidarIdentificador(codigo, nameof(codigo));
            categoriaDatos.Eliminar(codigo);
        }

        private static void ValidarCategoria(string codigo, string nombre)
        {
            ReglasLN.ValidarIdentificador(codigo, nameof(codigo));
            ReglasLN.ValidarObligatorio(nombre, nameof(nombre));
        }
    }
}