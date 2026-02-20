namespace MisRecetas.Web.Models;

/// <summary>
/// Proyección enriquecida de Receta para vistas y reportes (resultado de JOINs).
/// </summary>
public class RecetaDetalle
{
    public int Id_Receta { get; set; }
    public int Nro_Receta { get; set; }
    public int Id_Estado { get; set; }
    public string DescripcionEstado { get; set; } = string.Empty;

    // Médico
    public int Id_Medico { get; set; }
    public string NroMatricula { get; set; } = string.Empty;
    public string NombreMedico { get; set; } = string.Empty;
    public string ApellidoMedico { get; set; } = string.Empty;
    public string MedicoCompleto => $"{NombreMedico} {ApellidoMedico}".Trim();

    // Paciente
    public int Id_Paciente { get; set; }
    public string NroDocumento { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string NombrePaciente { get; set; } = string.Empty;
    public string ApellidoPaciente { get; set; } = string.Empty;
    public string PacienteCompleto => $"{NombrePaciente} {ApellidoPaciente}".Trim();

    // Fechas principales
    public DateTime? Fecha_Registro { get; set; }
    public DateTime? Fecha_Entrega_Medico { get; set; }
    public DateTime? Fecha_Recibe_Medico { get; set; }
    public DateTime? Fecha_Entrega_Paciente { get; set; }
    public DateTime? Fecha_Archivo { get; set; }
    public DateTime? Fecha_Desecho { get; set; }

    // Entrega a paciente
    public string? Nombre_Retira { get; set; }
    public string? Parentezco { get; set; }

    // Días en estado 3 (para archivar)
    public int DiasEnEstado3 { get; set; }
}
