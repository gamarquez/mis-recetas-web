using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.ViewModels;

public class RecibirDeMedicoViewModel
{
    [Display(Name = "Médico")]
    public int? IdMedicoFiltro { get; set; }

    public List<SelectListItem> ComboMedicos { get; set; } = [];

    // Recetas en estado 2
    public List<RecetaDetalle> Recetas { get; set; } = [];

    public List<int> NrosSeleccionados { get; set; } = [];
}
