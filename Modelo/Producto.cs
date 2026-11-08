namespace Modelo
{
public class Producto
{
    //Atributos 
    public string PrCodigo {get; set;}
    public string PrNombre {get; set;}
    public string PrDescripcion {get; set;}
    public string CategoriaProducto {get; set;}

    //Constructor
    public Producto(string PrCodigo, string PrNombre, string PrDescripcion, string CategoriaProducto)
    {
        this.PrCodigo = PrCodigo;
        this.PrNombre = PrNombre;
        this.PrDescripcion = PrDescripcion;
        this.CategoriaProducto = CategoriaProducto;
    }

    
}
}