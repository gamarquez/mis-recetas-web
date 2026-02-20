using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MisRecetas.Web.ViewModels;

public class PacienteViewModel
{
    public int Id_Paciente { get; set; }

    [Required(ErrorMessage = "Seleccione el tipo de documento.")]
    [Display(Name = "Tipo de documento")]
    public int Id_TipoDocumento { get; set; }

    [Required(ErrorMessage = "El número de documento es obligatorio.")]
    [Display(Name = "Nro. de documento")]
    [MaxLength(20)]
    public string Nro_Documento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre")]
    [MaxLength(100)]
    public string Nombre_Paciente { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [Display(Name = "Apellido")]
    [MaxLength(100)]
    public string Apellido_Paciente { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [MaxLength(200)]
    [EmailAddress(ErrorMessage = "Formato de email inválido.")]
    public string Email { get; set; } = string.Empty;

    public List<SelectListItem> ComboTipoDocumento { get; set; } = [];
}
