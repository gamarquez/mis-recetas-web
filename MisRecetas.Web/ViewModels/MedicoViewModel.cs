using System.ComponentModel.DataAnnotations;

namespace MisRecetas.Web.ViewModels;

public class MedicoViewModel
{
    public int Id_Medico { get; set; }

    [Required(ErrorMessage = "El número de matrícula es obligatorio.")]
    [Display(Name = "Nro. de matrícula")]
    [MaxLength(20)]
    public string Nro_Matricula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre")]
    [MaxLength(100)]
    public string Nombre_Medico { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [Display(Name = "Apellido")]
    [MaxLength(100)]
    public string Apellido_Medico { get; set; } = string.Empty;
}
