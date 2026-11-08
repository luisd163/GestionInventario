namespace Modelo
{
public class Categoria
{
    //Atributos 
    public string CCodigo {get; set;}
    public string CNombre {get; set;}

    //Constructor
    public Categoria(string CCodigo, string CNombre)
    {
        this.CCodigo = CCodigo;
        this.CNombre = CNombre;
    }

}
}