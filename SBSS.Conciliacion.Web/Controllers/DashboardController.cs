using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBSS.Conciliacion.Web.Models;
using SBSS.Conciliacion.Web.Repositories;

namespace SBSS.Conciliacion.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardRepository _dashboard;

    public DashboardController(IDashboardRepository dashboard) => _dashboard = dashboard;

    public async Task<IActionResult> Index(int? anio)
    {
        var anioConsulta = anio ?? DateTime.Today.Year;

        var vm = new DashboardViewModel
        {
            Resumen = await _dashboard.ObtenerResumenAsync(),
            PorMes = (await _dashboard.ObtenerPorMesAsync(anioConsulta)).ToList(),
            PorConciliador = (await _dashboard.ObtenerPorConciliadorAsync()).ToList(),
            TasaExito = (await _dashboard.ObtenerTasaExitoAsync()).ToList()
        };

        ViewBag.Anio = anioConsulta;
        return View(vm);
    }
}
