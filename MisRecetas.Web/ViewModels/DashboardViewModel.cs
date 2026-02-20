namespace MisRecetas.Web.ViewModels;

public class DashboardViewModel
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;

    // Contadores de estado para el panel de resumen
    public int TotalPendientes { get; set; }
    public int TotalConMedico { get; set; }
    public int TotalListasParaEntregar { get; set; }
    public int TotalEntregadas { get; set; }
    public int TotalArchivadas { get; set; }
    public int TotalDesechadas { get; set; }
    public int TotalParaArchivar { get; set; }   // Recetas en estado 3 con 7+ días
}
