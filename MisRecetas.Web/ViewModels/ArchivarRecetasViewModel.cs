using MisRecetas.Web.Models;

namespace MisRecetas.Web.ViewModels;

public class ArchivarRecetasViewModel
{
    // Recetas elegibles: estado 3 con 7+ días
    public List<RecetaDetalle> Recetas { get; set; } = [];
    public List<int> NrosSeleccionados { get; set; } = [];
}
