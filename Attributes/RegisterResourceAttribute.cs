namespace AiDbMaster.Attributes
{
    /// <summary>
    /// Attribute per marcare un controller come risorsa da registrare automaticamente
    /// nel sistema di gestione permessi.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisterResourceAttribute : Attribute
    {
        /// <summary>
        /// Nome univoco della risorsa (es: "AnagraficaClienti")
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Nome visualizzato nel menu (es: "Anagrafica Clienti")
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Descrizione della risorsa
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Icona Bootstrap Icons (es: "bi-people")
        /// </summary>
        public string? MenuIcon { get; set; }

        /// <summary>
        /// Ordine nel menu (più basso = più in alto)
        /// </summary>
        public int MenuOrder { get; set; } = 999;

        /// <summary>
        /// ID della risorsa parent (per sottomenu). 0 = nessun parent (root menu)
        /// </summary>
        public int ParentResourceId { get; set; } = 0;

        /// <summary>
        /// Se true, è un gruppo menu (non ha permessi CRUD)
        /// </summary>
        public bool IsMenuGroup { get; set; } = false;

        public RegisterResourceAttribute(string name, string displayName)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name non può essere vuoto", nameof(name));
            
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("DisplayName non può essere vuoto", nameof(displayName));

            Name = name;
            DisplayName = displayName;
        }
    }
}

