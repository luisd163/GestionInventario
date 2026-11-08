using System;
using Config;

namespace Logica
{
    public sealed class AuthLN
    {
        private readonly IAuthDatos authDatos;
        private readonly IBitacoraDatos bitacoraDatos;

        public AuthLN(IAuthDatos authDatos, IBitacoraDatos bitacoraDatos)
        {
            this.authDatos = authDatos ?? throw new ArgumentNullException(nameof(authDatos));
            this.bitacoraDatos = bitacoraDatos ?? throw new ArgumentNullException(nameof(bitacoraDatos));
        }

        public bool Autenticar(string identificador, string UContrasena)
        {
            ReglasLN.ValidarObligatorio(identificador, nameof(identificador));
            ReglasLN.ValidarObligatorio(UContrasena, nameof(UContrasena));

            if (SesionUsuario.EstaAutenticado)
            {
                throw new InvalidOperationException("Ya existe una sesión activa.");
            }

            UsuarioAutenticadoDatos usuario = authDatos.ValidarCredenciales(identificador, UContrasena);
            if (usuario == null)
            {
                return false;
            }

            ReglasLN.ValidarObligatorio(usuario.CodigoUsuario, nameof(usuario.CodigoUsuario));
            ReglasLN.ValidarObligatorio(usuario.NombreUsuario, nameof(usuario.NombreUsuario));
            if (usuario.CodigoRol != "ADM" && usuario.CodigoRol != "TRD" && usuario.CodigoRol != "ESP")
            {
                throw new InvalidOperationException("El usuario tiene un rol no reconocido.");
            }

            string codigoBitacora = bitacoraDatos.RegistrarEntrada(usuario.CodigoUsuario, DateTime.Now);
            ReglasLN.ValidarObligatorio(codigoBitacora, nameof(codigoBitacora));
            SesionUsuario.IniciarSesion(usuario.CodigoUsuario, usuario.NombreUsuario, usuario.CodigoRol, codigoBitacora);
            return true;
        }

        public void CerrarSesion()
        {
            ReglasLN.ExigirSesion();
            bitacoraDatos.RegistrarSalida(SesionUsuario.CodigoBitacora, DateTime.Now);
            SesionUsuario.FinalizarSesion();
        }
    }
}