namespace Modelo
{
public class ClasificacionProducto
{
    public string Producto{get; set;}
    public string CategoriaProducto{get; set;}

    public ClasificacionProducto(string Producto, string CategoriaProducto)
    {
        this.Producto = Producto;
        this.CategoriaProducto = CategoriaProducto;
    }
}
}