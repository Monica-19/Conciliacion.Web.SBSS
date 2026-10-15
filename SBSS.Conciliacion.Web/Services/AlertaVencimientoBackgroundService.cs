using Microsoft.AspNetCore.SignalR;
using SBSS.Conciliacion.Web.Hubs;
using SBSS.Conciliacion.Web.Repositories;

namespace SBSS.Conciliacion.Web.Services;

/// <summary>
/// Job en segundo plano dentro de la propia app: revisa periódicamente los
/// expedientes por vencer (sp_Notificacion_GenerarAlertasVencimiento) y las
/// empuja a los usuarios conectados por SignalR. Es la alternativa a programar
/// esto como un job de SQL Server Agent (ver comentario en BD-SBSS-SP.sql).
/// </summary>
public class AlertaVencimientoBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<NotificacionesHub> _hub;
    private readonly IConfiguration _config;
    private readonly ILogger<AlertaVencimientoBackgroundService> _logger;

    public AlertaVencimientoBackgroundService(
        IServiceScopeFactory scopeFactory,
        IHubContext<NotificacionesHub> hub,
        IConfiguration config,
        ILogger<AlertaVencimientoBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _hub = hub;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var diasAnticipacion = _config.GetValue("AlertasVencimiento:DiasAnticipacion", 5);
        var intervaloHoras = _config.GetValue("AlertasVencimiento:IntervaloRevisionHoras", 24);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var notificaciones = scope.ServiceProvider.GetRequiredService<INotificacionRepository>();

                var nuevas = await notificaciones.GenerarAlertasVencimientoAsync(diasAnticipacion);

                foreach (var notificacion in nuevas)
                {
                    var grupo = NotificacionesHub.GrupoDeUsuario(notificacion.IdUsuario.ToString());
                    await _hub.Clients.Group(grupo).SendAsync("NuevaNotificacion", notificacion, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando alertas de vencimiento de expedientes.");
            }

            await Task.Delay(TimeSpan.FromHours(intervaloHoras), stoppingToken);
        }
    }
}
