using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBSS.Conciliacion.Web.Models;
using SBSS.Conciliacion.Web.Repositories;

namespace SBSS.Conciliacion.Web.Controllers;

[Authorize]
public class SedesController : Controller
{
    private readonly ISedeRepository _sedes;

    public SedesController(ISedeRepository sedes) => _sedes = sedes;

    public async Task<IActionResult> Index()
    {
        var lista = await _sedes.ListarAsync();
        return View(lista);
    }

    [HttpGet]
    public IActionResult Create() => View(new CrearSedeVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearSedeVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await _sedes.InsertarAsync(vm);
        TempData["Ok"] = "Sede registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var sede = await _sedes.ObtenerPorIdAsync(id);
        if (sede is null) return NotFound();

        return View(new EditarSedeVm { IdSede = sede.IdSede, Nombre = sede.Nombre, Descripcion = sede.Descripcion });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditarSedeVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await _sedes.ActualizarAsync(vm);
        TempData["Ok"] = "Sede actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(CambiarEstadoSedeVm vm)
    {
        await _sedes.CambiarEstadoAsync(vm.IdSede, vm.EstadoSede);
        TempData["Ok"] = "Estado de la sede actualizado.";
        return RedirectToAction(nameof(Index));
    }
}
