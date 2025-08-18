using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AiDbMaster.ViewModels
{
    public class RoleViewModel
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public int UserCount { get; set; }
    }

    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "Il nome del ruolo è obbligatorio")]
        [Display(Name = "Nome Ruolo")]
        public string? Name { get; set; }
    }

    public class EditRoleViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Il nome del ruolo è obbligatorio")]
        [Display(Name = "Nome Ruolo")]
        public string? Name { get; set; }

        public List<UserRoleViewModel>? Users { get; set; }
    }

    public class UserRoleViewModel
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public bool IsInRole { get; set; }
    }
} 