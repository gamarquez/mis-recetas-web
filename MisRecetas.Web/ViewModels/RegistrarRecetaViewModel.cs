using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.ViewModels;

/// <summary>
/// ViewModel de dos pasos:
///   Paso 1 — buscar paciente por tipo+nro documento
///   Paso 2 — confirmar datos del paciente y registrar receta
/// </summary>
public class RegistrarRecetaViewModel
{
    // ── Paso 1: Búsqueda ──────────────────────────────────────────────────
    public int Paso { get; set; } = 1;

    [Display(Name = "Tipo de documento")]
    public int IdTipoDocumento { get; set; }

    [Display(Name = "Nro. de documento")]
    [MaxLength(20)]
    public string NroDocumento { get; set; } = string.Empty;

    // ── Paso 2: Datos del paciente (puede venir de la DB o ser nuevo) ─────
    public int? IdPacienteEncontrado { get; set; }  // null = paciente nuevo
    public bool PacienteNuevo { get; set; } = false;

    [Display(Name = "Nombre")]
    [MaxLength(100)]
    public string NombrePaciente { get; set; } = string.Empty;

    [Display(Name = "Apellido")]
    [MaxLength(100)]
    public string ApellidoPaciente { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [MaxLength(200)]
    [EmailAddress(ErrorMessage = "Formato de email inválido.")]
    public string EmailPaciente { get; set; } = string.Empty;

    // ── Paso 2: Datos de la receta ────────────────────────────────────────
    [Display(Name = "Médico")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un médico.")]
    public int IdMedico { get; set; }

    [Display(Name = "Nro. de receta")]
    [Range(1, int.MaxValue, ErrorMessage = "El número de receta debe ser mayor a 0.")]
    public int NroReceta { get; set; }

    // ── Combos ────────────────────────────────────────────────────────────
    public List<SelectListItem> ComboMedicos { get; set; } = [];
    public List<SelectListItem> ComboTipoDocumento { get; set; } = [];

    // ── Feedback de búsqueda ──────────────────────────────────────────────
    public string? MensajeBusqueda { get; set; }
}
