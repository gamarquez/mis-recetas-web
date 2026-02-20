using MisRecetas.Web.Models;

namespace MisRecetas.Web.ViewModels;

public class DesecharRecetasViewModel
{
    // Recetas en estado 5 (archivadas)
    public List<RecetaDetalle> Recetas { get; set; } = [];
    public List<int> NrosSeleccionados { get; set; } = [];
}
