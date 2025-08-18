using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AiDbMaster.ViewModels
{
    public class EditUserViewModel
    {
        public string? Id { get; set; }

        [Display(Name = "Nome utente")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "L'email è obbligatoria")]
        [EmailAddress(ErrorMessage = "Formato email non valido")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [Display(Name = "Nome")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Il cognome è obbligatorio")]
        [Display(Name = "Cognome")]
        public string? LastName { get; set; }

        [Display(Name = "Attivo")]
        public bool IsActive { get; set; }

        public List<UserRoleSelectionViewModel>? Roles { get; set; } = new List<UserRoleSelectionViewModel>();
    }

    public class UserRoleSelectionViewModel
    {
        public string? Name { get; set; }
        public bool IsSelected { get; set; }
    }
} 