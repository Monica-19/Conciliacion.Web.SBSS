using System.Data;
using Dapper;
using SBSS.Conciliacion.Web.Data;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public class NotificacionRepository : INotificacionRepository
{
    private readonly IDbConnectionFactory _factory;

    public NotificacionRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<int> CrearAsync(int idUsuario, int? idExpediente, string tipoNotificacion, string mensaje)
    {
        using var conn = _factory.CreateConnection();
        // sp_Notificacion_Crear devuelve el id nuevo mediante un SELECT (no un OUTPUT)
        var idNotificacion = await conn.QuerySingleAsync<int>(
            "sp_Notificacion_Crear",
            new { idUsuario, idExpediente, tipoNotificacion, mensaje },
            commandType: CommandType.StoredProcedure);
        return idNotificacion;
    }

    public async Task MarcarLeidaAsync(int idNotificacion)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_Notificacion_MarcarLeida",
            new { idNotificacion },
            commandType: CommandType.StoredProcedure);
    }

    public async Task MarcarTodasLeidasAsync(int idUsuario)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_Notificacion_MarcarTodasLeidas",
            new { idUsuario },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Notificacion>> ListarPorUsuarioAsync(int idUsuario, bool soloNoLeidas = false)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<Notificacion>(
            "sp_Notificacion_ListarPorUsuario",
            new { idUsuario, soloNoLeidas },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Notificacion>> GenerarAlertasVencimientoAsync(int diasAnticipacion)
    {
        using var conn = _factory.CreateConnection();
        // El SP ahora hace INSERT ... OUTPUT, así que esto ya no es un ExecuteAsync "mudo":
        // devuelve exactamente las filas que acaba de insertar.
        return await conn.QueryAsync<Notificacion>(
            "sp_Notificacion_GenerarAlertasVencimiento",
            new { diasAnticipacion },
            commandType: CommandType.StoredProcedure);
    }
}
