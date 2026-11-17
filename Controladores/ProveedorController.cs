using System;
using System.Collections.Generic;
using Logica;
using Modelo;

namespace Controladores
{
    public sealed class ProveedorController
    {
        private readonly ProveedorLN proveedorLN;

        public ProveedorController(ProveedorLN proveedorLN)
        {
            this.proveedorLN = proveedorLN ?? throw new ArgumentNullException(nameof(proveedorLN));
        }

        public IList<Proveedor> Listar()
        {
            return proveedorLN.Listar();
        }

        public Proveedor ObtenerPorCodigo(string pCodigo)
        {
            ValidacionLongitud.Exacta(pCodigo, 3, nameof(pCodigo));
            return proveedorLN.ObtenerPorCodigo(pCodigo.Trim());
        }

        public void Crear(string pCodigo, string pNombre, string pCiudad, string pDireccion)
        {
            ValidarProveedor(pCodigo, pNombre, pCiudad, pDireccion);
            proveedorLN.Crear(pCodigo.Trim(), pNombre.Trim(), pCiudad.Trim(), pDireccion.Trim());
        }

        public void Actualizar(string pCodigo, string pNombre, string pCiudad, string pDireccion)
        {
            ValidarProveedor(pCodigo, pNombre, pCiudad, pDireccion);
            proveedorLN.Actualizar(pCodigo.Trim(), pNombre.Trim(), pCiudad.Trim(), pDireccion.Trim());
        }

        public void Eliminar(string pCodigo)
        {
            ValidacionLongitud.Exacta(pCodigo, 3, nameof(pCodigo));
            proveedorLN.Eliminar(pCodigo.Trim());
        }

        public IList<CatalogoProveedor> ObtenerCatalogo(string pCodigo)
        {
            ValidacionLongitud.Exacta(pCodigo, 3, nameof(pCodigo));
            return proveedorLN.ObtenerCatalogo(pCodigo.Trim());
        }

        // Nota: el diccionario permite CaCantidad >= 0; ReglasLN.ValidarPositivo y CatalogoProveedorCD exigen > 0 (pendiente de ajustar).
        public void AgregarProductoCatalogo(string pCodigo, string prCodigo, int cantidad, decimal precio)
        {
            ValidacionLongitud.Exacta(pCodigo, 3, nameof(pCodigo));
            ValidacionLongitud.Exacta(prCodigo, 3, nameof(prCodigo));
            proveedorLN.AgregarProductoCatalogo(pCodigo.Trim(), prCodigo.Trim(), cantidad, precio);
        }

        public void ActualizarProductoCatalogo(string pCodigo, string prCodigo, int cantidad, decimal precio)
        {
            ValidacionLongitud.Exacta(pCodigo, 3, nameof(pCodigo));
            ValidacionLongitud.Exacta(prCodigo, 3, nameof(prCodigo));
            proveedorLN.ActualizarProductoCatalogo(pCodigo.Trim(), prCodigo.Trim(), cantidad, precio);
        }

        public void EliminarProductoCatalogo(string pCodigo, string prCodigo)
        {
            ValidacionLongitud.Exacta(pCodigo, 3, nameof(pCodigo));
            ValidacionLongitud.Exacta(prCodigo, 3, nameof(prCodigo));
            proveedorLN.EliminarProductoCatalogo(pCodigo.Trim(), prCodigo.Trim());
        }

        private static void ValidarProveedor(string pCodigo, string pNombre, string pCiudad, string pDireccion)
        {
            ValidacionLongitud.Exacta(pCodigo, 3, nameof(pCodigo));
            ValidacionLongitud.Maxima(pNombre, 50, nameof(pNombre));
            ValidacionLongitud.Maxima(pCiudad, 30, nameof(pCiudad));
            ValidacionLongitud.Maxima(pDireccion, 100, nameof(pDireccion));
        }
    }
}