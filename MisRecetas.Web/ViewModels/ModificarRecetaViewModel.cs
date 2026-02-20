using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.ViewModels;

public class ModificarRecetaViewModel
{
    // Búsqueda
    public string? NroRecetaBuscar { get; set; }

    // Receta encontrada
    public RecetaDetalle? RecetaActual { get; set; }

    [Display(Name = "Médico")]
    public int IdMedico { get; set; }

    [Display(Name = "Estado")]
    [Range(1, 6)]
    public int IdEstado { get; set; }

    // Combos
    public List<SelectListItem> ComboMedicos { get; set; } = [];
    public List<SelectListItem> ComboEstados { get; set; } = [];
}
