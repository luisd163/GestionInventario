using System;

namespace Modelo
{
public class Bitacora
{
    //Atributos
    public string BCodigo { get; set; }
    public DateTime BTiempoIngreso { get; set; }
    public DateTime? BTiempoSalida { get; set; }
    public string Usuario { get; set; }
    //Constructor
    public Bitacora(string BCodigo, DateTime BTiempoIngreso, DateTime? BTiempoSalida, string Usuario)
    {
        this.BCodigo = BCodigo;
        this.BTiempoIngreso = BTiempoIngreso;
        this.BTiempoSalida = BTiempoSalida;
        this.Usuario = Usuario;
    }

}
}