namespace MisRecetas.Web.Models;

public class Medico
{
    public int Id_Medico { get; set; }
    public string Nro_Matricula { get; set; } = string.Empty;
    public string Nombre_Medico { get; set; } = string.Empty;
    public string Apellido_Medico { get; set; } = string.Empty;

    public string NombreCompleto => $"{Nombre_Medico} {Apellido_Medico}".Trim();
}
