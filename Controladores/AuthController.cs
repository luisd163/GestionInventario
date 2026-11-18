using System;
using Config;
using Logica;

namespace Controladores
{
    public sealed class AuthController
    {
        private readonly AuthLN authLN;

        public AuthController(AuthLN authLN)
        {
            this.authLN = authLN ?? throw new ArgumentNullException(nameof(authLN));
        }

        public bool IniciarSesion(string uCodigo, string uContrasena)
        {
            ValidacionLongitud.Exacta(uCodigo, 3, nameof(uCodigo));
            ValidacionLongitud.Maxima(uContrasena, 15, nameof(uContrasena));
            return authLN.Autenticar(uCodigo.Trim(), uContrasena);
        }

        public void CerrarSesion()
        {
            authLN.CerrarSesion();
        }

        public string RolActual()
        {
            return SesionUsuario.CodigoRol;
        }

        public bool PuedeEscribirCatalogos()
        {
            return SesionUsuario.EstaAutenticado && (SesionUsuario.CodigoRol == "001" || SesionUsuario.CodigoRol == "002");
        }

        public bool EsAdministrador()
        {
            return SesionUsuario.EstaAutenticado && SesionUsuario.CodigoRol == "001";
        }
    }
}