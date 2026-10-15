using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBSS.Conciliacion.Web.Models;
using SBSS.Conciliacion.Web.Repositories;
using SBSS.Conciliacion.Web.Services;

namespace SBSS.Conciliacion.Web.Controllers;

[Authorize]
public class AudienciasController : Controller
{
    private readonly IAudienciaRepository _audiencias;
    private readonly IExpedienteRepository _expedientes;
    private readonly IDocumentoWordService _word;
    private readonly IUsuarioRepository _usuarios;
    private readonly INotificacionRepository _notificaciones;
    private readonly ITrabajadorRepository _trabajadores;
    private readonly IDocumentoRepository _documentos;

    public AudienciasController(
        IAudienciaRepository audiencias,
        IExpedienteRepository expedientes,
        IDocumentoWordService word,
        IUsuarioRepository usuarios,
        INotificacionRepository notificaciones,
        ITrabajadorRepository trabajadores,
        IDocumentoRepository documentos)
    {
        _audiencias = audiencias;
        _expedientes = expedientes;
        _word = word;
        _usuarios = usuarios;
        _notificaciones = notificaciones;
        _trabajadores = trabajadores;
        _documentos = documentos;
    }

    // GET: /Audiencias/Programar?idExpediente=5
    [HttpGet]
    public async Task<IActionResult> Programar(int idExpediente)
    {
        var expediente = await _expedientes.ObtenerPorIdAsync(idExpediente);
        if (expediente is null) return NotFound();

        await CargarCatalogosAsync();

        return View(new ProgramarAudienciaVm
        {
            IdExpediente = idExpediente,
            NumeroExpediente = expediente.Expediente.NumeroExpediente
        });
    }

    // POST: /Audiencias/Programar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Programar(ProgramarAudienciaVm vm)
    {
        if (!ModelState.IsValid)
        {
            await CargarCatalogosAsync();
            return View(vm);
        }

        await _audiencias.ProgramarAsync(vm);

        var idUsuario = await _usuarios.ObtenerIdUsuarioPorTrabajadorAsync(vm.IdConciliador);
        if (idUsuario.HasValue)
        {
            await _notificaciones.CrearAsync(
                idUsuario.Value,
                vm.IdExpediente,
                "AUDIENCIA",
                $"Se te programó una audiencia para el expediente {vm.NumeroExpediente} el {vm.Fecha:dd/MM/yyyy} a las {vm.Hora:hh\\:mm}.");
        }

        TempData["Ok"] = "Audiencia programada correctamente.";
        return RedirectToAction("Details", "Expedientes", new { id = vm.IdExpediente });
    }

    // GET: /Audiencias/Details/12
    public async Task<IActionResult> Details(int id)
    {
        var audiencia = await _audiencias.ObtenerPorIdAsync(id);
        if (audiencia is null) return NotFound();
        return View(audiencia);
    }

    // GET: /Audiencias/Reprogramar/12
    [HttpGet]
    public IActionResult Reprogramar(int idDocumento, int idExpediente)
    {
        return View(new ReprogramarAudienciaVm { IdDocumento = idDocumento, IdExpediente = idExpediente });
    }

    // POST: /Audiencias/Reprogramar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reprogramar(ReprogramarAudienciaVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await _audiencias.ReprogramarAsync(vm);
        TempData["Ok"] = "Audiencia reprogramada correctamente.";
        return RedirectToAction("Details", "Expedientes", new { id = vm.IdExpediente });
    }

    // GET: /Audiencias/RegistrarResultado
    [HttpGet]
    public IActionResult RegistrarResultado(int idDocumento, int idExpediente)
    {
        return View(new RegistrarResultadoVm { IdDocumento = idDocumento, IdExpediente = idExpediente });
    }

    // POST: /Audiencias/RegistrarResultado
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarResultado(RegistrarResultadoVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await _audiencias.RegistrarResultadoAsync(vm);
        TempData["Ok"] = "Resultado de audiencia registrado correctamente.";
        return RedirectToAction("Details", "Expedientes", new { id = vm.IdExpediente });
    }

    // GET: /Audiencias/DescargarCitacion?idExpediente=5&idDocumento=12
    public async Task<IActionResult> DescargarCitacion(ProgramarAudienciaVm vm)
    {
        var detalle = await _expedientes.ObtenerPorIdAsync(vm.IdExpediente);
        if (detalle is null) return NotFound();

        var archivo = _word.GenerarCitacionAudiencia(detalle.Expediente, vm);
        var nombre = $"Citacion_{detalle.Expediente.NumeroExpediente}.docx";
        return File(archivo, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nombre);
    }

    private async Task CargarCatalogosAsync()
    {
        ViewBag.Conciliadores = (await _trabajadores.ListarAsync(soloActivos: true)).ToList();
        ViewBag.TiposDocumento = (await _documentos.ListarTiposDocumentoAsync()).ToList();
    }
}
