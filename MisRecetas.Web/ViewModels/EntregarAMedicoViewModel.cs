using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.ViewModels;

public class EntregarAMedicoViewModel
{
    // Filtro por médico (opcional)
    [Display(Name = "Filtrar por médico")]
    public int? IdMedicoFiltro { get; set; }

    public List<SelectListItem> ComboMedicos { get; set; } = [];

    // Recetas en estado 1
    public List<RecetaDetalle> Recetas { get; set; } = [];

    // Nros de receta seleccionados con checkbox
    public List<int> NrosSeleccionados { get; set; } = [];
}
