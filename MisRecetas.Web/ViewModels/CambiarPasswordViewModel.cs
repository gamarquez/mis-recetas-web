using System.ComponentModel.DataAnnotations;

namespace MisRecetas.Web.ViewModels;

public class CambiarPasswordViewModel
{
    [Required(ErrorMessage = "Ingrese su contraseña actual.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña actual")]
    public string PasswordActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese la nueva contraseña.")]
    [MinLength(6, ErrorMessage = "Mínimo 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string NuevaPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme la nueva contraseña.")]
    [Compare(nameof(NuevaPassword), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarPassword { get; set; } = string.Empty;
}
