using System;
using System.Collections.Generic;
using Logica;
using Modelo;

namespace Controladores
{
    public sealed class UsuarioController
    {
        private readonly UsuarioLN usuarioLN;
        private readonly RolUsuarioLN rolLN;

        public UsuarioController(UsuarioLN usuarioLN, RolUsuarioLN rolLN)
        {
            this.usuarioLN = usuarioLN ?? throw new ArgumentNullException(nameof(usuarioLN));
            this.rolLN = rolLN ?? throw new ArgumentNullException(nameof(rolLN));
        }

        public IList<Usuario> Listar()
        {
            return usuarioLN.Listar();
        }

        public Usuario ObtenerPorCodigo(string uCodigo)
        {
            ValidacionLongitud.Exacta(uCodigo, 3, nameof(uCodigo));
            return usuarioLN.ObtenerPorCodigo(uCodigo.Trim());
        }

        public void Crear(string uNombre, string uCodigo, string uContrasena, string codigoRol)
        {
            Validar(uNombre, uCodigo, uContrasena, codigoRol);
            usuarioLN.Crear(uNombre.Trim(), uCodigo.Trim(), uContrasena, codigoRol.Trim());
        }

        public void Actualizar(string uNombre, string uCodigo, string uContrasena, string codigoRol)
        {
            Validar(uNombre, uCodigo, uContrasena, codigoRol);
            usuarioLN.Actualizar(uNombre.Trim(), uCodigo.Trim(), uContrasena, codigoRol.Trim());
        }

        public void Eliminar(string uCodigo)
        {
            ValidacionLongitud.Exacta(uCodigo, 3, nameof(uCodigo));
            usuarioLN.Eliminar(uCodigo.Trim());
        }

        public IList<RolUsuario> ListarRoles()
        {
            return rolLN.Listar();
        }

        private static void Validar(string uNombre, string uCodigo, string uContrasena, string codigoRol)
        {
            ValidacionLongitud.Exacta(uCodigo, 3, nameof(uCodigo));
            ValidacionLongitud.Maxima(uNombre, 50, nameof(uNombre));
            ValidacionLongitud.Maxima(uContrasena, 15, nameof(uContrasena));
            ValidacionLongitud.Exacta(codigoRol, 3, nameof(codigoRol));
        }
    }
}