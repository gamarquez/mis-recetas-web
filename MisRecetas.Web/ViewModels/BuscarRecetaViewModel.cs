using System.ComponentModel.DataAnnotations;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.ViewModels;

public class BuscarRecetaViewModel
{
    // ── Filtros ───────────────────────────────────────────────────────────
    [Display(Name = "Nro. de receta")]
    public string? NroReceta { get; set; }

    [Display(Name = "Nombre del paciente")]
    public string? Nombre { get; set; }

    [Display(Name = "Apellido del paciente")]
    public string? Apellido { get; set; }

    [Display(Name = "Nro. de documento")]
    public string? Documento { get; set; }

    // ── Resultados ────────────────────────────────────────────────────────
    public List<RecetaDetalle> Resultados { get; set; } = [];
    public bool BusquedaRealizada { get; set; } = false;
}
