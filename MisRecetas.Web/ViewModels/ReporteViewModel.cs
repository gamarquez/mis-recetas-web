using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.ViewModels;

public class ReporteViewModel
{
    [Display(Name = "Medico")]
    public int? IdMedico { get; set; }

    [Display(Name = "Estado")]
    public int? IdEstado { get; set; }

    [Display(Name = "Desde")]
    [DataType(DataType.Date)]
    public DateTime? FechaDesde { get; set; }

    [Display(Name = "Hasta")]
    [DataType(DataType.Date)]
    public DateTime? FechaHasta { get; set; }

    public string? NroReceta { get; set; }

    public List<SelectListItem> ComboMedicos { get; set; } = [];
    public List<SelectListItem> ComboEstados { get; set; } = [];

    public List<RecetaDetalle> Datos { get; set; } = [];
    public string TituloReporte { get; set; } = string.Empty;
    public string SubtituloReporte { get; set; } = string.Empty;

    public bool BusquedaRealizada { get; set; }
}
