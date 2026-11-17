using System;
using System.Collections.Generic;
using Logica;
using Modelo;

namespace Controladores
{
    public sealed class CategoriaController
    {
        private readonly CategoriaLN categoriaLN;

        public CategoriaController(CategoriaLN categoriaLN)
        {
            this.categoriaLN = categoriaLN ?? throw new ArgumentNullException(nameof(categoriaLN));
        }

        public IList<Categoria> Listar()
        {
            return categoriaLN.Listar();
        }

        public Categoria ObtenerPorCodigo(string cCodigo)
        {
            ValidacionLongitud.Exacta(cCodigo, 3, nameof(cCodigo));
            return categoriaLN.ObtenerPorCodigo(cCodigo.Trim());
        }

        public void Crear(string cCodigo, string cNombre)
        {
            Validar(cCodigo, cNombre);
            categoriaLN.Crear(cCodigo.Trim(), cNombre.Trim());
        }

        public void Actualizar(string cCodigo, string cNombre)
        {
            Validar(cCodigo, cNombre);
            categoriaLN.Actualizar(cCodigo.Trim(), cNombre.Trim());
        }

        public void Eliminar(string cCodigo)
        {
            ValidacionLongitud.Exacta(cCodigo, 3, nameof(cCodigo));
            categoriaLN.Eliminar(cCodigo.Trim());
        }

        private static void Validar(string cCodigo, string cNombre)
        {
            ValidacionLongitud.Exacta(cCodigo, 3, nameof(cCodigo));
            ValidacionLongitud.Maxima(cNombre, 30, nameof(cNombre));
        }
    }
}