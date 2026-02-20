namespace MisRecetas.Web.Models;

public class Usuario
{
    public int Id_Usuario { get; set; }
    public string Nombre_Usuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Id_Rol { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;

    // Navegación (no mapeada a DB directamente)
    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
    public string DescripcionRol { get; set; } = string.Empty;
}
