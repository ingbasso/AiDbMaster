using System.ComponentModel.DataAnnotations;

namespace AiDbMaster.ViewModels
{
    public class ChangePasswordViewModel
    {
        public string? UserId { get; set; }
        
        [Display(Name = "Nome Utente")]
        public string? UserName { get; set; }
        
        [Display(Name = "Email")]
        public string? Email { get; set; }
        
        [Display(Name = "Nome Completo")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "La nuova password è obbligatoria")]
        [StringLength(100, ErrorMessage = "La {0} deve essere lunga almeno {2} caratteri.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Nuova Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Conferma Nuova Password")]
        [Compare("NewPassword", ErrorMessage = "Le password non corrispondono.")]
        public string? ConfirmPassword { get; set; }
    }
} 