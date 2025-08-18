using System.ComponentModel.DataAnnotations;

namespace AiDbMaster.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "L'email è obbligatoria")]
        [EmailAddress(ErrorMessage = "Formato email non valido")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La password è obbligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Display(Name = "Ricordami")]
        public bool RememberMe { get; set; }
    }
} 