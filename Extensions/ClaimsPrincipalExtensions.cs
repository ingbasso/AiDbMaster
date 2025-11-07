using System.Security.Claims;

namespace AiDbMaster.Extensions
{
    /// <summary>
    /// Extension methods per ClaimsPrincipal per semplificare l'accesso ai dati utente.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Ottiene l'ID dell'utente corrente
        /// </summary>
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// Ottiene il nome utente corrente
        /// </summary>
        public static string? GetUserName(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>
        /// Ottiene l'email dell'utente corrente
        /// </summary>
        public static string? GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Verifica se l'utente è Admin
        /// </summary>
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.IsInRole("Admin");
        }

        /// <summary>
        /// Verifica se l'utente è un Agente
        /// </summary>
        public static bool IsAgente(this ClaimsPrincipal user)
        {
            return user.IsInRole("Agenti");
        }
    }
}

