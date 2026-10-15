using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBSS.Conciliacion.Web.Repositories;

namespace SBSS.Conciliacion.Web.Controllers;

/// <summary>Endpoints ligeros (fetch desde las vistas) para los combos en cascada Departamento → Provincia → Distrito.</summary>
[Authorize]
[Route("api/ubigeo")]
public class UbigeoController : Controller
{
    private readonly IUbigeoRepository _ubigeo;

    public UbigeoController(IUbigeoRepository ubigeo) => _ubigeo = ubigeo;

    [HttpGet("departamentos")]
    public async Task<IActionResult> Departamentos() => Ok(await _ubigeo.ListarDepartamentosAsync());

    [HttpGet("provincias/{idDepartamento:int}")]
    public async Task<IActionResult> Provincias(int idDepartamento) => Ok(await _ubigeo.ListarProvinciasAsync(idDepartamento));

    [HttpGet("distritos/{idProvincia:int}")]
    public async Task<IActionResult> Distritos(int idProvincia) => Ok(await _ubigeo.ListarDistritosAsync(idProvincia));
}
