using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBSS.Conciliacion.Web.Models;
using SBSS.Conciliacion.Web.Repositories;

namespace SBSS.Conciliacion.Web.Controllers;

[Authorize]
public class MateriasController : Controller
{
    private readonly IMateriaRepository _materias;

    public MateriasController(IMateriaRepository materias) => _materias = materias;

    public async Task<IActionResult> Index()
    {
        var lista = await _materias.ListarMateriasAsync();
        return View(lista);
    }

    [HttpGet]
    public IActionResult CreateTipo() => View(new CrearTipoMateriaVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTipo(CrearTipoMateriaVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await _materias.InsertarTipoAsync(vm);
        TempData["Ok"] = "Tipo de materia registrado correctamente.";
        return RedirectToAction(nameof(Create));
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await CargarTiposAsync();
        return View(new CrearMateriaVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearMateriaVm vm)
    {
        if (!ModelState.IsValid)
        {
            await CargarTiposAsync();
            return View(vm);
        }

        await _materias.InsertarMateriaAsync(vm);
        TempData["Ok"] = "Materia registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var materia = await _materias.ObtenerMateriaPorIdAsync(id);
        if (materia is null) return NotFound();

        await CargarTiposAsync();
        return View(new EditarMateriaVm
        {
            IdMateria = materia.IdMateria,
            IdTipoMateria = materia.IdTipoMateria,
            NombreMateria = materia.NombreMateria,
            DescripcionMateria = materia.DescripcionMateria
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditarMateriaVm vm)
    {
        if (!ModelState.IsValid)
        {
            await CargarTiposAsync();
            return View(vm);
        }

        await _materias.ActualizarMateriaAsync(vm);
        TempData["Ok"] = "Materia actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var materia = await _materias.ObtenerMateriaPorIdAsync(id);
        if (materia is null) return NotFound();

        return View("Eliminar", materia);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, IFormCollection form)
    {
        var exito = await _materias.EliminarMateriaAsync(id);

        if (exito)
        {
            TempData["Ok"] = "Materia eliminada correctamente.";
        }
        else
        {
            TempData["Error"] = "No se puede eliminar esta materia, porque esta asociada aun expediente.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarTiposAsync()
    {
        ViewBag.Tipos = (await _materias.ListarTiposAsync()).ToList();
    }
}
