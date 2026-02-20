using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MisRecetas.Web.ViewModels;

public class UsuarioViewModel
{
    public int Id_Usuario { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [Display(Name = "Usuario")]
    [MaxLength(50)]
    public string Nombre_Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [Display(Name = "Apellido")]
    [MaxLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione un rol.")]
    [Display(Name = "Rol")]
    public int Id_Rol { get; set; }

    // Solo en creación
    [Display(Name = "Contraseña")]
    [MinLength(6, ErrorMessage = "Mínimo 6 caracteres.")]
    public string? Password { get; set; }

    [Display(Name = "Confirmar contraseña")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    public string? ConfirmarPassword { get; set; }

    public List<SelectListItem> ComboRoles { get; set; } = [];
    public string DescripcionRol { get; set; } = string.Empty;
    public bool EsEdicion => Id_Usuario > 0;
}
