using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SBSS.Conciliacion.Web.Hubs;

/// <summary>
/// Hub de notificaciones en tiempo real. Cada usuario conectado se une a un grupo
/// "usuario-{idUsuario}", así el servidor puede enviarle solo sus propias alertas
/// (vencimientos, audiencias asignadas, cambios de estado, etc.).
/// </summary>
[Authorize]
public class NotificacionesHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var idUsuario = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(idUsuario))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GrupoDeUsuario(idUsuario));
        }

        await base.OnConnectedAsync();
    }

    public static string GrupoDeUsuario(string idUsuario) => $"usuario-{idUsuario}";
}
