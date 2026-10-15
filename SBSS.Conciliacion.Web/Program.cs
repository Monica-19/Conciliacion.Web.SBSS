using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using SBSS.Conciliacion.Web.Data;
using SBSS.Conciliacion.Web.Hubs;
using SBSS.Conciliacion.Web.Repositories;
using SBSS.Conciliacion.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------
// Servicios MVC + SignalR
// ---------------------------------------------------------------
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

// Límite de subida alineado con el máximo de video (200 MB) definido en
// TiposArchivoAdjunto.TamanioMaximoBytes. Si se hospeda detrás de IIS,
// además hay que subir maxAllowedContentLength en web.config (su default
// es ~30 MB e ignora esta configuración de Kestrel).
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 200L * 1024 * 1024;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 200L * 1024 * 1024;
});

// ---------------------------------------------------------------
// Acceso a datos (Dapper sobre SQL Server, llamando a los SP)
// ---------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection' en appsettings.json");

builder.Services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IExpedienteRepository, ExpedienteRepository>();
builder.Services.AddScoped<IAudienciaRepository, AudienciaRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<INotificacionRepository, NotificacionRepository>();
builder.Services.AddScoped<IDocumentoRepository, DocumentoRepository>();
builder.Services.AddScoped<ISedeRepository, SedeRepository>();
builder.Services.AddScoped<ITrabajadorRepository, TrabajadorRepository>();
builder.Services.AddScoped<IMateriaRepository, MateriaRepository>();
builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IReporteRepository, ReporteRepository>();
builder.Services.AddScoped<IUbigeoRepository, UbigeoRepository>();

builder.Services.AddScoped<IDocumentoWordService, DocumentoWordService>();
builder.Services.AddScoped<IReporteExcelService, ReporteExcelService>();
builder.Services.AddScoped<IReportePdfService, ReportePdfService>();
builder.Services.AddScoped<IArchivoAlmacenamientoService, ArchivoAlmacenamientoService>();

// Cliente tipado para la API de Decolecta (DNI/RUC). La API key se lee de
// Decolecta:ApiKey (configurarla con User Secrets o variables de entorno,
// nunca en appsettings.json en el repositorio).
builder.Services.AddHttpClient<IDecolectaService, DecolectaService>();

// Job en segundo plano: revisa vencimientos y notifica por SignalR (alternativa a un job de SQL Agent)
builder.Services.AddHostedService<AlertaVencimientoBackgroundService>();

// ---------------------------------------------------------------
// Autenticación por cookie (basada en Usuario / Trabajador de la BD)
// ---------------------------------------------------------------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "SBSS.Auth";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ---------------------------------------------------------------
// Pipeline HTTP
// ---------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapHub<NotificacionesHub>("/hubs/notificaciones");

app.Run();
