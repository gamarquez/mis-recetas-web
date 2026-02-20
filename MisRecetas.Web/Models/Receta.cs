namespace MisRecetas.Web.Models;

public class Receta
{
    public int Id_Receta { get; set; }
    public int Nro_Receta { get; set; }
    public int Id_Medico { get; set; }
    public int Id_Paciente { get; set; }
    public int Id_Estado { get; set; }

    // Auditoría: Registro
    public int? Id_Usuario_Registra { get; set; }
    public DateTime? Fecha_Usuario_Registra { get; set; }

    // Auditoría: Entrega a médico
    public int? Id_Usuario_Entrega_A_Medico { get; set; }
    public DateTime? Fecha_Usuario_Entrega_A_Medico { get; set; }

    // Auditoría: Recibe de médico
    public int? Id_Usuario_Recibe_De_Medico { get; set; }
    public DateTime? Fecha_Usuario_Recibe_De_Medico { get; set; }

    // Auditoría: Entrega a paciente
    public int? Id_Usuario_Entrega_A_Paciente { get; set; }
    public DateTime? Fecha_Usuario_Entrega_A_Paciente { get; set; }
    public string? Nombre_Retira { get; set; }
    public string? Parentezco { get; set; }

    // Auditoría: Archivo
    public int? Id_Usuario_Entrega_A_Archivo { get; set; }
    public DateTime? Fecha_Entrega_A_Archivo { get; set; }

    // Auditoría: Desecho
    public int? Id_Usuario_Desecha { get; set; }
    public DateTime? Fecha_Usuario_Desecha { get; set; }

    // Navegación (join en queries)
    public string NombreMedico { get; set; } = string.Empty;
    public string ApellidoMedico { get; set; } = string.Empty;
    public string NombrePaciente { get; set; } = string.Empty;
    public string ApellidoPaciente { get; set; } = string.Empty;
    public string DescripcionEstado { get; set; } = string.Empty;

    public string MedicoCompleto => $"{NombreMedico} {ApellidoMedico}".Trim();
    public string PacienteCompleto => $"{NombrePaciente} {ApellidoPaciente}".Trim();
}
