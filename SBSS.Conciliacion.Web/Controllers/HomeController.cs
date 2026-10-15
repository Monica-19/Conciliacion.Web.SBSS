using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SBSS.Conciliacion.Web.Controllers;

/// <summary>
/// Solo maneja errores. El sistema es de uso interno exclusivo del
/// personal del centro (no hay sitio público ni landing de cara al
/// cliente): el punto de entrada es directamente Account/Login.
/// </summary>
public class HomeController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(Activity.Current?.Id ?? HttpContext.TraceIdentifier);
}
