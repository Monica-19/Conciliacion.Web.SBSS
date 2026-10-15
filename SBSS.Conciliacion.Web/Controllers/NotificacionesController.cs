using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBSS.Conciliacion.Web.Repositories;

namespace SBSS.Conciliacion.Web.Controllers;

[Authorize]
public class NotificacionesController : Controller
{
    private readonly INotificacionRepository _notificaciones;

    public NotificacionesController(INotificacionRepository notificaciones) => _notificaciones = notificaciones;

    // GET: /Notificaciones — panel completo (no solo el conteo de la campanita)
    public async Task<IActionResult> Index()
    {
        var idUsuario = ObtenerIdUsuario();
        var todas = (await _notificaciones.ListarPorUsuarioAsync(idUsuario, soloNoLeidas: false))
            .OrderByDescending(n => n.FechaCreacion)
            .ToList();

        return View(todas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarLeida(int id)
    {
        await _notificaciones.MarcarLeidaAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarTodasLeidas()
    {
        await _notificaciones.MarcarTodasLeidasAsync(ObtenerIdUsuario());
        TempData["Ok"] = "Se marcaron todas las notificaciones como leídas.";
        return RedirectToAction(nameof(Index));
    }

    // ---- Endpoints ligeros que consume la campanita (site.js) vía fetch ----

    [HttpGet("api/notificaciones")]
    public async Task<IActionResult> Listar(bool soloNoLeidas = true)
    {
        var idUsuario = ObtenerIdUsuario();
        var lista = await _notificaciones.ListarPorUsuarioAsync(idUsuario, soloNoLeidas);
        return Json(lista);
    }

    [HttpPost("api/notificaciones/{id:int}/marcar-leida")]
    public async Task<IActionResult> MarcarLeidaApi(int id)
    {
        await _notificaciones.MarcarLeidaAsync(id);
        return Ok();
    }

    private int ObtenerIdUsuario() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
}
