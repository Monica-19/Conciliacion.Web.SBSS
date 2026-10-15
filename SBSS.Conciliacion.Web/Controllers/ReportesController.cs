using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBSS.Conciliacion.Web.Models;
using SBSS.Conciliacion.Web.Repositories;
using SBSS.Conciliacion.Web.Services;

namespace SBSS.Conciliacion.Web.Controllers;

[Authorize]
public class ReportesController : Controller
{
    private readonly IReporteRepository _reportes;
    private readonly IMateriaRepository _materias;
    private readonly IReporteExcelService _excel;
    private readonly IReportePdfService _pdf;

    public ReportesController(
        IReporteRepository reportes,
        IMateriaRepository materias,
        IReporteExcelService excel,
        IReportePdfService pdf)
    {
        _reportes = reportes;
        _materias = materias;
        _excel = excel;
        _pdf = pdf;
    }

    public IActionResult Index() => View();

    // GET: /Reportes/PorRango?fechaInicio=&fechaFin=
    public async Task<IActionResult> PorRango(FiltroReporteRangoVm filtro)
    {
        var resultado = ModelState.IsValid
            ? (await _reportes.ObtenerPorRangoAsync(filtro.FechaInicio, filtro.FechaFin)).ToList()
            : new List<ReporteExpedienteVm>();

        ViewBag.Filtro = filtro;
        return View(resultado);
    }

    public async Task<IActionResult> PorRangoExcel(FiltroReporteRangoVm filtro)
    {
        var datos = await _reportes.ObtenerPorRangoAsync(filtro.FechaInicio, filtro.FechaFin);
        var archivo = _excel.GenerarReportePorRango(datos);
        return File(archivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Expedientes_{filtro.FechaInicio:yyyyMMdd}_{filtro.FechaFin:yyyyMMdd}.xlsx");
    }

    public async Task<IActionResult> PorRangoPdf(FiltroReporteRangoVm filtro)
    {
        var datos = await _reportes.ObtenerPorRangoAsync(filtro.FechaInicio, filtro.FechaFin);
        var archivo = _pdf.GenerarReportePorRango(datos);
        return File(archivo, "application/pdf",
            $"Expedientes_{filtro.FechaInicio:yyyyMMdd}_{filtro.FechaFin:yyyyMMdd}.pdf");
    }

    // GET: /Reportes/PorMateria?idTipoMateria=
    public async Task<IActionResult> PorMateria(int? idTipoMateria)
    {
        var resultado = (await _reportes.ObtenerPorMateriaAsync(idTipoMateria)).ToList();
        ViewBag.Tipos = (await _materias.ListarTiposAsync()).ToList();
        ViewBag.IdTipoMateria = idTipoMateria;
        return View(resultado);
    }

    public async Task<IActionResult> PorMateriaExcel(int? idTipoMateria)
    {
        var datos = await _reportes.ObtenerPorMateriaAsync(idTipoMateria);
        var archivo = _excel.GenerarReportePorMateria(datos);
        return File(archivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExpedientesPorMateria.xlsx");
    }

    public async Task<IActionResult> PorMateriaPdf(int? idTipoMateria)
    {
        var datos = await _reportes.ObtenerPorMateriaAsync(idTipoMateria);
        var archivo = _pdf.GenerarReportePorMateria(datos);
        return File(archivo, "application/pdf", "ExpedientesPorMateria.pdf");
    }
}
