using System;

namespace Modelo
{
    public class AuditoriaAccesoDTO
    {
        public string UNombre { get; set; }
        public string RNombre { get; set; }
        public DateTime BTiempoIngreso { get; set; }
        public DateTime? BTiempoSalida { get; set; }
    }
}