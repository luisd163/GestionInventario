namespace Modelo
{
public class Usuario
{
    //Atributos
    public string UNombre { get; set; }
    public string UCodigo { get; set; }
    public string UContrasena { get; set; }
    public string RolUsuario { get; set; }

    //Constructor
    public Usuario(string UNombre, string UCodigo, string UContrasena, string RolUsuario)
    {
        this.UNombre = UNombre;
        this.UCodigo = UCodigo;
        this.UContrasena = UContrasena;
        this.RolUsuario = RolUsuario;
    }

}
}