using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBSS.Conciliacion.Web.Models;
using SBSS.Conciliacion.Web.Repositories;

namespace SBSS.Conciliacion.Web.Controllers;

[Authorize]
public class PersonasController : Controller
{
    private readonly IPersonaRepository _personas;

    public PersonasController(IPersonaRepository personas) => _personas = personas;

    public async Task<IActionResult> Index(string? busqueda)
    {
        var lista = await _personas.ListarAsync(busqueda);
        ViewBag.Busqueda = busqueda;
        return View(lista);
    }

    public async Task<IActionResult> Details(int id)
    {
        var persona = await _personas.ObtenerPorIdAsync(id);
        if (persona is null) return NotFound();

        ViewBag.Direccion = await _personas.ObtenerDireccionPrincipalAsync(id);
        return View(persona);
    }

    [HttpGet]
    public IActionResult CreateNatural() => View(new CrearPersonaNaturalVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNatural(CrearPersonaNaturalVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var idPersona = await _personas.InsertarNaturalAsync(vm);
        await GuardarDireccionSiCorrespondeAsync(idPersona, vm.Direccion);

        TempData["Ok"] = "Persona natural registrada correctamente.";
        return RedirectToAction(nameof(Details), new { id = idPersona });
    }

    [HttpGet]
    public IActionResult CreateJuridica() => View(new CrearPersonaJuridicaVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJuridica(CrearPersonaJuridicaVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var idPersona = await _personas.InsertarJuridicaAsync(vm);
        await GuardarDireccionSiCorrespondeAsync(idPersona, vm.Direccion);

        TempData["Ok"] = "Persona jurídica registrada correctamente.";
        return RedirectToAction(nameof(Details), new { id = idPersona });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(CambiarEstadoPersonaVm vm)
    {
        await _personas.CambiarEstadoAsync(vm.IdPersona, vm.EstadoPersona);
        TempData["Ok"] = "Estado actualizado.";
        return RedirectToAction(nameof(Details), new { id = vm.IdPersona });
    }

    /// <summary>
    /// La dirección es opcional y se guarda en un paso separado (Direccion +
    /// DetalleDireccion), después de crear la persona. Si esto llegara a
    /// fallar, la persona ya quedó creada igual — no es una operación
    /// atómica con el alta de la persona, a propósito: es metadata
    /// adicional, no un dato indispensable del registro.
    /// </summary>
    private async Task GuardarDireccionSiCorrespondeAsync(int idPersona, DireccionVm direccion)
    {
        if (!direccion.TieneDatos || !direccion.IdDistrito.HasValue) return;

        var idDireccion = await _personas.InsertarDireccionAsync(direccion);
        await _personas.VincularDireccionAsync(idPersona, idDireccion);
    }
}
