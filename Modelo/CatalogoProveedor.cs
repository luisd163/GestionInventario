namespace Modelo
{
    public class CatalogoProveedor
    {
        public string Proveedor{ get; set; }
        public string Producto { get; set; }
        public int CaCantidad { get; set; }
        public decimal CaPrecio { get; set; }

        public CatalogoProveedor(string Proveedor, string Producto, int CaCantidad, decimal CaPrecio)
        {
            this.Proveedor = Proveedor;
            this.Producto = Producto;
            this.CaCantidad = CaCantidad;
            this.CaPrecio = CaPrecio;
        }
    }
}