using AiDbMaster.Data;
using AiDbMaster.Hubs;
using AiDbMaster.Models;
using AiDbMaster.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurazione della connessione al database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configurazione di Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information));

// Configurazione di Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Registrazione dei servizi personalizzati
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<PermissionService>();

// Registrazione del servizio HttpClient per Mistral AI
builder.Services.AddHttpClient<MistralAIService>();
builder.Services.AddScoped<MistralAIService>();

// Registra il servizio di monitoraggio delle cartelle come singleton
// in modo che possa essere iniettato sia come IHostedService che come servizio normale
builder.Services.AddSingleton<FolderMonitorService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<FolderMonitorService>());

// Registra il servizio di notifica della catalogazione
builder.Services.AddScoped<CatalogNotificationService>();

// Aggiungi SignalR
builder.Services.AddSignalR();

// Registra il servizio DatabaseQuery
builder.Services.AddScoped<DatabaseQuery>();

// Configurazione dei cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
});

// Configurazione del client HTTP per OpenAI
builder.Services.AddHttpClient("OpenAI", client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", builder.Configuration["OpenAI:ApiKey"]);
});

// Configurazione CORS per dominio pubblico
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionCorsPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Security:AllowedOrigins").Get<string[]>() ?? 
                            new[] { "https://www.aidocmaster.it", "https://aidocmaster.it" };
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configurazione sicurezza aggiuntiva per produzione
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(365);
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Attiva CORS per dominio pubblico
app.UseCors("ProductionCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Configura gli endpoint di SignalR
app.MapHub<CatalogHub>("/catalogHub");

// Seed del database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Applica le migrazioni e crea il database se non esiste
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        
        // Seed dei ruoli e dell'utente amministratore
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Si è verificato un errore durante l'inizializzazione del database.");
    }
}

app.Run();
