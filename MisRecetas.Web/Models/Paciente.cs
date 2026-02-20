namespace MisRecetas.Web.Models;

public class Paciente
{
    public int Id_Paciente { get; set; }
    public int Id_TipoDocumento { get; set; }
    public string Nro_Documento { get; set; } = string.Empty;
    public string Nombre_Paciente { get; set; } = string.Empty;
    public string Apellido_Paciente { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Navegación
    public string DescripcionTipoDocumento { get; set; } = string.Empty;
    public string NombreCompleto => $"{Nombre_Paciente} {Apellido_Paciente}".Trim();
}
