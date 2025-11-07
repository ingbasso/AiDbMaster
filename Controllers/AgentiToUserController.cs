using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la conversione degli agenti in utenti
    /// Permette di creare account utente per gli agenti esistenti
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AgentiToUserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AgentiToUserController> _logger;

        public AgentiToUserController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AgentiToUserController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// Pagina principale per la gestione della conversione agenti in utenti
        /// </summary>
        public IActionResult Index()
        {
            ViewBag.UseFluidContainer = true; // Usa container-fluid per tutta larghezza
            return View();
        }

        /// <summary>
        /// Recupera la lista di tutti gli agenti con informazione se sono già stati convertiti
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAgenti()
        {
            try
            {
                // Recupera tutti gli agenti
                var agenti = await _context.TabellaAgenti
                    .OrderBy(a => a.CodiceAgente)
                    .ToListAsync();

                // Recupera tutti gli utenti che hanno un CodiceAgente associato
                var utentiAgenti = await _context.Users
                    .Where(u => u.CodiceAgente != null)
                    .Select(u => u.CodiceAgente)
                    .ToListAsync();

                // Crea la lista con informazioni sullo stato di conversione
                var agentiConStato = agenti.Select(a => new
                {
                    a.CodiceAgente,
                    a.DescrizioneAgente,
                    a.IndirizzoAgente,
                    a.CapAgente,
                    a.CittaAgente,
                    a.ProvinciaAgente,
                    a.Attivo,
                    StatoAgente = a.StatoAgente,
                    StatoAgenteCssClass = a.Attivo ? "bg-success" : "bg-secondary",
                    IsConverted = utentiAgenti.Contains(a.CodiceAgente),
                    IsConvertedCssClass = utentiAgenti.Contains(a.CodiceAgente) ? "bg-success" : "bg-warning",
                    IsConvertedText = utentiAgenti.Contains(a.CodiceAgente) ? "Già Utente" : "Non Convertito",
                    ButtonCssClass = utentiAgenti.Contains(a.CodiceAgente) ? "btn-secondary" : "btn-primary",
                    ButtonDisabled = utentiAgenti.Contains(a.CodiceAgente) ? "disabled" : "",
                    ButtonText = utentiAgenti.Contains(a.CodiceAgente) ? "Già Convertito" : "Converti",
                    IndirizzoCompleto = a.IndirizzoCompleto
                }).ToList();

                return Json(agentiConStato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli agenti");
                return StatusCode(500, new { error = "Errore durante il recupero degli agenti" });
            }
        }

        /// <summary>
        /// Verifica se un agente è già stato convertito in utente
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckIfConverted(short codiceAgente)
        {
            try
            {
                var exists = await _context.Users
                    .AnyAsync(u => u.CodiceAgente == codiceAgente);

                return Json(new { isConverted = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica dell'agente {CodiceAgente}", codiceAgente);
                return StatusCode(500, new { error = "Errore durante la verifica" });
            }
        }

        /// <summary>
        /// Converte un agente in utente creando un account con ruolo "Agenti"
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertToUser([FromBody] ConvertAgenteRequest request)
        {
            try
            {
                // Validazione input
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { error = "L'email è obbligatoria" });
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { error = "La password è obbligatoria" });
                }

                // Verifica che l'agente esista
                var agente = await _context.TabellaAgenti
                    .FirstOrDefaultAsync(a => a.CodiceAgente == request.CodiceAgente);

                if (agente == null)
                {
                    return NotFound(new { error = $"Agente con codice {request.CodiceAgente} non trovato" });
                }

                // Verifica che l'agente non sia già stato convertito
                var esisteUtente = await _context.Users
                    .AnyAsync(u => u.CodiceAgente == request.CodiceAgente);

                if (esisteUtente)
                {
                    return BadRequest(new { error = "Questo agente è già stato convertito in utente" });
                }

                // Verifica che l'email non sia già in uso
                var emailInUso = await _userManager.FindByEmailAsync(request.Email);
                if (emailInUso != null)
                {
                    return BadRequest(new { error = "L'email è già in uso da un altro utente" });
                }

                // Crea il nuovo utente
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName ?? "Agente",
                    LastName = request.LastName ?? agente.CodiceAgente.ToString(),
                    CodiceAgente = request.CodiceAgente,
                    IsActive = agente.Attivo,
                    EmailConfirmed = true // Confermiamo l'email automaticamente
                };

                // Crea l'utente con la password fornita
                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new { error = $"Errore durante la creazione dell'utente: {errors}" });
                }

                // Verifica che il ruolo "Agenti" esista, altrimenti crealo
                if (!await _roleManager.RoleExistsAsync(UserRoles.Agenti))
                {
                    await _roleManager.CreateAsync(new IdentityRole(UserRoles.Agenti));
                }

                // Assegna il ruolo "Agenti" all'utente
                await _userManager.AddToRoleAsync(user, UserRoles.Agenti);

                _logger.LogInformation(
                    "Agente {CodiceAgente} convertito in utente con email {Email}",
                    request.CodiceAgente,
                    request.Email);

                return Ok(new
                {
                    success = true,
                    message = "Utente creato con successo",
                    userId = user.Id,
                    email = user.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la conversione dell'agente {CodiceAgente}", request.CodiceAgente);
                return StatusCode(500, new { error = $"Errore durante la conversione: {ex.Message}" });
            }
        }
    }

    /// <summary>
    /// Modello per la richiesta di conversione agente in utente
    /// </summary>
    public class ConvertAgenteRequest
    {
        public short CodiceAgente { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}

