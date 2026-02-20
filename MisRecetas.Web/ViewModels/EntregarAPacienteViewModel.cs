using System.ComponentModel.DataAnnotations;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.ViewModels;

public class EntregarAPacienteViewModel
{
    // Recetas en estado 3
    public List<RecetaDetalle> Recetas { get; set; } = [];

    // Receta seleccionada para entregar
    public int NroRecetaSeleccionada { get; set; }

    [Required(ErrorMessage = "Ingrese el nombre de quien retira.")]
    [Display(Name = "Nombre de quien retira")]
    [MaxLength(150)]
    public string NombreRetira { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese el parentesco.")]
    [Display(Name = "Parentesco")]
    [MaxLength(80)]
    public string Parentezco { get; set; } = string.Empty;
}
