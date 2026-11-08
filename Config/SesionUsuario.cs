using System;

namespace Config
{
    public static class SesionUsuario
    {
        public static string CodigoUsuario { get; private set; }
        public static string NombreUsuario { get; private set; }
        public static string CodigoRol { get; private set; }
        public static string CodigoBitacora { get; private set; }
        public static bool EstaAutenticado
        {
            get { return !string.IsNullOrWhiteSpace(CodigoUsuario); }
        }

        internal static void IniciarSesion(string codigoUsuario, string nombreUsuario, string codigoRol, string codigoBitacora)
        {
            if (EstaAutenticado)
            {
                throw new InvalidOperationException("Ya existe una sesión activa.");
            }

            CodigoUsuario = codigoUsuario;
            NombreUsuario = nombreUsuario;
            CodigoRol = codigoRol;
            CodigoBitacora = codigoBitacora;
        }

        internal static void FinalizarSesion()
        {
            CodigoUsuario = null;
            NombreUsuario = null;
            CodigoRol = null;
            CodigoBitacora = null;
        }
    }
}