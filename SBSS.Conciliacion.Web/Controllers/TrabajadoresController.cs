using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBSS.Conciliacion.Web.Models;
using SBSS.Conciliacion.Web.Repositories;

namespace SBSS.Conciliacion.Web.Controllers;

[Authorize]
public class TrabajadoresController : Controller
{
    private readonly ITrabajadorRepository _trabajadores;
    private readonly ISedeRepository _sedes;
    private readonly IUsuarioRepository _usuarios;

    public TrabajadoresController(
        ITrabajadorRepository trabajadores,
        ISedeRepository sedes,
        IUsuarioRepository usuarios)
    {
        _trabajadores = trabajadores;
        _sedes = sedes;
        _usuarios = usuarios;
    }

    public async Task<IActionResult> Index()
    {
        var lista = await _trabajadores.ListarAsync();
        return View(lista);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await CargarSedesAsync();
        return View(new CrearTrabajadorVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearTrabajadorVm vm)
    {
        if (!ModelState.IsValid)
        {
            await CargarSedesAsync();
            return View(vm);
        }

        var idTrabajador = await _trabajadores.InsertarAsync(vm);
        TempData["Ok"] = "Trabajador registrado correctamente.";
        return RedirectToAction(nameof(Details), new { id = idTrabajador });
    }

    public async Task<IActionResult> Details(int id)
    {
        var trabajador = await _trabajadores.ObtenerPorIdAsync(id);
        if (trabajador is null) return NotFound();

        var idUsuario = await _usuarios.ObtenerIdUsuarioPorTrabajadorAsync(id);
        ViewBag.TieneAcceso = idUsuario.HasValue;
        return View(trabajador);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var trabajador = await _trabajadores.ObtenerPorIdAsync(id);
        if (trabajador is null) return NotFound();

        await CargarSedesAsync();
        return View(new EditarTrabajadorVm
        {
            IdTrabajador = trabajador.IdTrabajador,
            IdSedeRef = trabajador.IdSedeRef ?? 0,
            NombreTrabajador = trabajador.NombreTrabajador,
            ApePaterTrabajador = trabajador.ApePaterTrabajador,
            ApeMaterTrabajador = trabajador.ApeMaterTrabajador,
            EmailTrabajador = trabajador.EmailTrabajador,
            TelefonoTrabajador = trabajador.TelefonoTrabajador
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditarTrabajadorVm vm)
    {
        if (!ModelState.IsValid)
        {
            await CargarSedesAsync();
            return View(vm);
        }

        await _trabajadores.ActualizarAsync(vm);
        TempData["Ok"] = "Trabajador actualizado correctamente.";
        return RedirectToAction(nameof(Details), new { id = vm.IdTrabajador });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(CambiarEstadoTrabajadorVm vm)
    {
        await _trabajadores.CambiarEstadoAsync(vm.IdTrabajador, vm.EstadoTrabajador);
        TempData["Ok"] = "Estado del trabajador actualizado.";
        return RedirectToAction(nameof(Details), new { id = vm.IdTrabajador });
    }

    // GET: /Trabajadores/CrearAcceso/5
    [HttpGet]
    public async Task<IActionResult> CrearAcceso(int id)
    {
        var trabajador = await _trabajadores.ObtenerPorIdAsync(id);
        if (trabajador is null) return NotFound();

        return View(new CrearAccesoVm { IdTrabajador = id, NombreTrabajador = trabajador.NombreCompleto });
    }

    // POST: /Trabajadores/CrearAcceso
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearAcceso(CrearAccesoVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var hash = BCrypt.Net.BCrypt.HashPassword(vm.Contrasenia);
        await _usuarios.InsertarAsync(vm.IdTrabajador, vm.Usuario, hash, vm.Rol);

        TempData["Ok"] = $"Se creó el acceso para {vm.Usuario}.";
        return RedirectToAction(nameof(Details), new { id = vm.IdTrabajador });
    }

    private async Task CargarSedesAsync()
    {
        ViewBag.Sedes = (await _sedes.ListarAsync(soloActivas: true)).ToList();
    }
}
