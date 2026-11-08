using System;
using System.Collections.Generic;
using Modelo;

namespace Logica
{
    public sealed class UsuarioLN
    {
        private readonly IUsuarioDatos usuarioDatos;

        public UsuarioLN(IUsuarioDatos usuarioDatos)
        {
            this.usuarioDatos = usuarioDatos ?? throw new ArgumentNullException(nameof(usuarioDatos));
        }

        public IList<Usuario> Listar()
        {
            ReglasLN.ExigirSesion();
            return usuarioDatos.Listar();
        }

        public Usuario ObtenerPorCodigo(string codigo)
        {
            ReglasLN.ExigirSesion();
            ReglasLN.ValidarIdentificador(codigo, nameof(codigo));
            return usuarioDatos.ObtenerPorCodigo(codigo);
        }

        public void Crear(string UNombre, string UCodigo, string UContrasena, string RolUsuario)
        {
            ReglasLN.ExigirAdministrador();
            ValidarUsuario(UNombre, UCodigo, RolUsuario);
            ReglasLN.ValidarObligatorio(UContrasena, nameof(UContrasena));
            usuarioDatos.Crear(new Usuario(UNombre, UCodigo, UContrasena, RolUsuario), UContrasena);
        }

        public void Actualizar(string UNombre, string UCodigo, string UContrasena, string RolUsuario)
        {
            ReglasLN.ExigirAdministrador();
            ValidarUsuario(UNombre, UCodigo, RolUsuario);
            ReglasLN.ValidarObligatorio(UContrasena, nameof(UContrasena));
            usuarioDatos.Actualizar(new Usuario(UNombre, UCodigo, UContrasena, RolUsuario), UContrasena);
        }

        public void Eliminar(string codigo)
        {
            ReglasLN.ExigirAdministrador();
            ReglasLN.ValidarIdentificador(codigo, nameof(codigo));
            usuarioDatos.Eliminar(codigo);
        }

        private static void ValidarUsuario(string nombre, string codigo, string codigoRol)
        {
            ReglasLN.ValidarObligatorio(nombre, nameof(nombre));
            ReglasLN.ValidarIdentificador(codigo, nameof(codigo));
            ReglasLN.ValidarIdentificador(codigoRol, nameof(codigoRol));
            if (codigoRol != "ADM" && codigoRol != "TRD" && codigoRol != "ESP")
            {
                throw new ArgumentException("El rol debe ser ADM, TRD o ESP.", nameof(codigoRol));
            }
        }
    }
}