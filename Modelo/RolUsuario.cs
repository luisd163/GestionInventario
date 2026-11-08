namespace Modelo
{
public class RolUsuario{

    //Atributos Rol Usuario
    private string RCodigo {get; set;} 
    private string RNombre {get; set;}

    //Constructor
    public RolUsuario(string RCodigo, string RNombre) {
        this.RCodigo = RCodigo;
        this.RNombre = RNombre;
    }
}
}