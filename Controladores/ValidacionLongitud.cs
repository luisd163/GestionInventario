using System;

namespace Controladores
{
    // Restricciones de longitud del Diccionario de Datos (char(n) / varchar(n)).
    internal static class ValidacionLongitud
    {
        internal static void Exacta(string valor, int n, string campo)
        {
            if (valor == null || valor.Trim().Length != n)
            {
                throw new ArgumentException("El campo " + campo + " debe tener exactamente " + n + " caracteres.", campo);
            }
        }

        internal static void Maxima(string valor, int n, string campo)
        {
            if (valor != null && valor.Length > n)
            {
                throw new ArgumentException("El campo " + campo + " no puede superar " + n + " caracteres.", campo);
            }
        }
    }
}