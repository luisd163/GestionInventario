using System;
using System.Text.RegularExpressions;
using Config;

namespace Logica
{
    internal static class ReglasLN
    {
        private static readonly Regex FormatoIdentificador = new Regex("^[A-Za-z0-9_-]+$");

        internal static void ExigirSesion()
        {
            if (!SesionUsuario.EstaAutenticado)
            {
                throw new UnauthorizedAccessException("Debe iniciar sesión para realizar esta operación.");
            }
        }

        internal static void ExigirAdministrador()
        {
            ExigirSesion();
            if (SesionUsuario.CodigoRol != "ADM")
            {
                throw new UnauthorizedAccessException("Esta operación requiere permisos de administrador.");
            }
        }

        internal static void ExigirEscrituraCatalogos()
        {
            ExigirSesion();
            if (SesionUsuario.CodigoRol != "ADM" && SesionUsuario.CodigoRol != "TRD")
            {
                throw new UnauthorizedAccessException("El rol actual solo tiene permiso de consulta.");
            }
        }

        internal static void ValidarObligatorio(string valor, string campo)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new ArgumentException("El campo " + campo + " es obligatorio.", campo);
            }
        }

        internal static void ValidarIdentificador(string valor, string campo)
        {
            ValidarObligatorio(valor, campo);
            if (!FormatoIdentificador.IsMatch(valor))
            {
                throw new ArgumentException("El campo " + campo + " solo puede contener letras, números, guion y guion bajo.", campo);
            }
        }

        internal static void ValidarPositivo(int valor, string campo)
        {
            if (valor <= 0)
            {
                throw new ArgumentOutOfRangeException(campo, "El valor debe ser mayor que cero.");
            }
        }

        internal static void ValidarPositivo(decimal valor, string campo)
        {
            if (valor <= 0)
            {
                throw new ArgumentOutOfRangeException(campo, "El valor debe ser mayor que cero.");
            }
        }

        internal static void ValidarRangoFechas(DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
            {
                throw new ArgumentException("La fecha inicial no puede ser posterior a la fecha final.");
            }
        }
    }
}