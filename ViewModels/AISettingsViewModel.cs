using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AiDbMaster.ViewModels
{
    public class AISettingsViewModel
    {
        [Required(ErrorMessage = "La chiave API di Mistral AI è obbligatoria")]
        [Display(Name = "Chiave API Mistral AI")]
        public string? MistralApiKey { get; set; }

        [Required(ErrorMessage = "L'endpoint API di Mistral AI è obbligatorio")]
        [Display(Name = "Endpoint API Mistral AI")]
        public string? MistralApiEndpoint { get; set; }

        [Required(ErrorMessage = "Il nome del modello Mistral AI è obbligatorio")]
        [Display(Name = "Nome Modello Mistral AI")]
        public string? MistralModelName { get; set; }

        [Display(Name = "Cartelle Monitorate")]
        public List<string> MonitoredFolders { get; set; } = new List<string>();

        [Required(ErrorMessage = "L'ID utente predefinito è obbligatorio")]
        [Display(Name = "Utente Predefinito")]
        public string? DefaultUserId { get; set; }

        [Display(Name = "Utenti Disponibili")]
        public List<UserViewModel> AvailableUsers { get; set; } = new List<UserViewModel>();
    }
} 