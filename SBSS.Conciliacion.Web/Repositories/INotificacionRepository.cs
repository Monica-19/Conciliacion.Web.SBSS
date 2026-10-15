using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public interface INotificacionRepository
{
    Task<int> CrearAsync(int idUsuario, int? idExpediente, string tipoNotificacion, string mensaje);
    Task MarcarLeidaAsync(int idNotificacion);
    Task MarcarTodasLeidasAsync(int idUsuario);
    Task<IEnumerable<Notificacion>> ListarPorUsuarioAsync(int idUsuario, bool soloNoLeidas = false);

    /// <summary>
    /// Genera las alertas de vencimiento pendientes y devuelve las notificaciones
    /// recién creadas, para que quien la invoque (el job en segundo plano) las
    /// empuje de inmediato por SignalR en vez de dejarlas solo en la tabla.
    /// </summary>
    Task<IEnumerable<Notificacion>> GenerarAlertasVencimientoAsync(int diasAnticipacion);
}
