namespace Modelo
{
public class Proveedor
{

    //Atributos
    public string PCodigo {  get; set; }
    public string PNombre {  get; set; }
    public string PCiudad {  get; set; }
    public string PDireccion {  get; set; }

    //Constructor
    public Proveedor(string PCodigo, string PNombre, string PCiudad, string PDireccion)
    {
        this.PCodigo = PCodigo;
        this.PNombre = PNombre;
        this.PCiudad = PCiudad;
        this.PDireccion = PDireccion;
    }

}
}