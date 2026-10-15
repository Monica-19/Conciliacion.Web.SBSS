using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SBSS.Conciliacion.Web.Controllers;

/// <summary>Landing simple para llegar a los catálogos (Sedes, Trabajadores, Materias, Personas).</summary>
[Authorize]
public class CatalogosController : Controller
{
    public IActionResult Index() => View();
}
