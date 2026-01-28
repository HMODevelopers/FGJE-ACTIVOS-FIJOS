using System.ComponentModel.DataAnnotations;

namespace ActivosFijos.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
        [Display(Name = "Contraseña actual")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [Display(Name = "Nueva contraseña")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
        [Display(Name = "Confirmar nueva contraseña")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }

        public string ReturnUrl { get; set; }
    }
}
