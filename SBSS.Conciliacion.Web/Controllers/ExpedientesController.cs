using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using SBSS.Conciliacion.Web.Models;
using SBSS.Conciliacion.Web.Repositories;
using SBSS.Conciliacion.Web.Services;

namespace SBSS.Conciliacion.Web.Controllers;

[Authorize]
public class ExpedientesController : Controller
{
    private readonly IExpedienteRepository _expedientes;
    private readonly IAudienciaRepository _audiencias;
    private readonly IReporteExcelService _excel;
    private readonly IReportePdfService _pdf;
    private readonly IUsuarioRepository _usuarios;
    private readonly INotificacionRepository _notificaciones;
    private readonly ISedeRepository _sedes;
    private readonly ITrabajadorRepository _trabajadores;
    private readonly IPersonaRepository _personas;
    private readonly IMateriaRepository _materias;
    private readonly IDocumentoRepository _documentos;
    private readonly IArchivoAlmacenamientoService _almacenamiento;

    public ExpedientesController(
        IExpedienteRepository expedientes,
        IAudienciaRepository audiencias,
        IReporteExcelService excel,
        IReportePdfService pdf,
        IUsuarioRepository usuarios,
        INotificacionRepository notificaciones,
        ISedeRepository sedes,
        ITrabajadorRepository trabajadores,
        IPersonaRepository personas,
        IMateriaRepository materias,
        IDocumentoRepository documentos,
        IArchivoAlmacenamientoService almacenamiento)
    {
        _expedientes = expedientes;
        _audiencias = audiencias;
        _excel = excel;
        _pdf = pdf;
        _usuarios = usuarios;
        _notificaciones = notificaciones;
        _sedes = sedes;
        _trabajadores = trabajadores;
        _personas = personas;
        _materias = materias;
        _documentos = documentos;
        _almacenamiento = almacenamiento;
    }

 
    public async Task<IActionResult> Index(ExpedienteFiltroVm filtro)
    {
        var lista = await _expedientes.ListarAsync(filtro);
        ViewBag.Filtro = filtro;

        var materiasRaw = await _materias.ListarMateriasAsync();

        var materiasAgrupadas = materiasRaw
            .Select(m => {
                char separador = m.NombreMateria.Contains('–') ? '–' :
                                 m.NombreMateria.Contains('-') ? '-' : ';';

                if (m.NombreMateria.Contains(separador))
                {
                    var partes = m.NombreMateria.Split(separador);
                    return new
                    {
                        Id = m.IdMateria,
                        Categoria = partes.Length > 0 ? partes[0].Trim() : "Otras Materias",
                        Detalle = partes.Length > 1 ? partes[1].Trim() : m.NombreMateria.Trim()
                    };
                }
                else
                {
                    return new
                    {
                        Id = m.IdMateria,
                        Categoria = "Otras Materias",
                        Detalle = m.NombreMateria.Trim()
                    };
                }
            })
            .GroupBy(m => m.Categoria)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => (dynamic)new { x.Id, x.Detalle }).ToList()
            );



        ViewBag.MateriasGrouped = materiasAgrupadas;

        return View(lista);
    }


    public async Task<IActionResult> Details(int id)
    {
        var detalle = await _expedientes.ObtenerPorIdAsync(id);
        if (detalle is null) return NotFound();

        detalle.Audiencias = (await _audiencias.ListarPorExpedienteAsync(id)).ToList();
        detalle.Archivos = (await _documentos.ListarArchivosPorExpedienteAsync(id)).ToList();

        ViewBag.Personas = (await _personas.ListarAsync()).ToList();
        ViewBag.Materias = (await _materias.ListarMateriasAsync()).ToList();
        return View(detalle);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await CargarCatalogosAsync();
        return View(new CrearExpedienteVm { FechaRecepcion = DateTime.Today, PlazoDias = 30 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearExpedienteVm vm)
    {
        if (!ModelState.IsValid)
        {
            await CargarCatalogosAsync();
            return View(vm);
        }

        var idExpediente = await _expedientes.InsertarAsync(vm);
        TempData["Ok"] = "Expediente registrado correctamente.";
        return RedirectToAction(nameof(Details), new { id = idExpediente });
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarConciliador(int idExpediente)
    {
        int idConciliadorAsignado;
        try
        {
            idConciliadorAsignado = await _expedientes.AsignarConciliadorAleatorioAsync(idExpediente);
        }
        catch (Exception)
        {
            TempData["Error"] = "No se pudo asignar un conciliador: no hay trabajadores activos disponibles.";
            return RedirectToAction(nameof(Details), new { id = idExpediente });
        }

        var idUsuario = await _usuarios.ObtenerIdUsuarioPorTrabajadorAsync(idConciliadorAsignado);
        var detalle = await _expedientes.ObtenerPorIdAsync(idExpediente);

        if (idUsuario.HasValue)
        {
            await _notificaciones.CrearAsync(
                idUsuario.Value,
                idExpediente,
                "ASIGNACION",
                $"Se te asignó el expediente {detalle?.Expediente.NumeroExpediente} como conciliador.");
        }

        TempData["Ok"] = $"Se asignó a {detalle?.Expediente.NombreConciliador} como conciliador (asignación aleatoria).";
        return RedirectToAction(nameof(Details), new { id = idExpediente });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(CambiarEstadoVm vm)
    {
        await _expedientes.CambiarEstadoAsync(vm.IdExpediente, vm.NuevoEstado);
        TempData["Ok"] = $"El expediente cambió de estado a {vm.NuevoEstado}.";
        return RedirectToAction(nameof(Details), new { id = vm.IdExpediente });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarPersona(AgregarPersonaExpedienteVm vm)
    {
        if (!ModelState.IsValid || vm.IdPersona <= 0)
        {
            TempData["Error"] = "Selecciona una persona y un rol antes de vincular.";
            return RedirectToAction(nameof(Details), new { id = vm.IdExpediente });
        }

        await _expedientes.AgregarPersonaAsync(vm);
        TempData["Ok"] = "Persona vinculada al expediente.";
        return RedirectToAction(nameof(Details), new { id = vm.IdExpediente });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarMateria(AgregarMateriaExpedienteVm vm)
    {
        if (!ModelState.IsValid || vm.IdMateria <= 0)
        {
            TempData["Error"] = "Selecciona una materia antes de vincular. Si el combo está vacío, primero registra materias en Catálogos → Materias.";
            return RedirectToAction(nameof(Details), new { id = vm.IdExpediente });
        }

        await _expedientes.AgregarMateriaAsync(vm);
        TempData["Ok"] = "Materia vinculada al expediente.";
        return RedirectToAction(nameof(Details), new { id = vm.IdExpediente });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _expedientes.EliminarAsync(id);
        TempData["Ok"] = "Expediente eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(200L * 1024 * 1024)]
    public async Task<IActionResult> SubirArchivo(SubirArchivoVm vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Selecciona un archivo y un tipo válidos.";
            return RedirectToAction(nameof(Details), new { id = vm.IdExpediente });
        }

        var resultado = await _almacenamiento.GuardarAsync(vm.IdExpediente, vm.TipoArchivo, vm.Archivo);
        if (!resultado.Exito)
        {
            TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Details), new { id = vm.IdExpediente });
        }

        var idTrabajador = int.Parse(User.FindFirstValue("idTrabajador") ?? "0");
        await _documentos.InsertarArchivoAdjuntoAsync(
            vm.IdExpediente, idTrabajador, vm.TipoArchivo, vm.Archivo.FileName, resultado.RutaRelativa!, resultado.TamanioKB);

        TempData["Ok"] = "Archivo subido correctamente.";
        return RedirectToAction(nameof(Details), new { id = vm.IdExpediente });
    }

    public async Task<IActionResult> DescargarArchivo(int id)
    {
        var archivo = await _documentos.ObtenerArchivoPorIdAsync(id);
        if (archivo is null) return NotFound();

        var rutaFisica = _almacenamiento.ObtenerRutaFisica(archivo.RutaArchivo);
        if (!System.IO.File.Exists(rutaFisica)) return NotFound();

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(archivo.NombreArchivo, out var contentType))
            contentType = "application/octet-stream";

        var bytes = await System.IO.File.ReadAllBytesAsync(rutaFisica);
        return File(bytes, contentType, archivo.NombreArchivo);
    }

    public async Task<IActionResult> ExportarExcel(ExpedienteFiltroVm filtro)
    {
        var lista = await _expedientes.ListarAsync(filtro);
        var archivo = _excel.GenerarReporteExpedientes(lista);
        var nombre = $"Expedientes_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        return File(archivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombre);
    }

    public async Task<IActionResult> ExportarPdf(ExpedienteFiltroVm filtro)
    {
        var lista = await _expedientes.ListarAsync(filtro);
        var archivo = _pdf.GenerarReporteExpedientes(lista);
        var nombre = $"Expedientes_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
        return File(archivo, "application/pdf", nombre);
    }

    private async Task CargarCatalogosAsync()
    {
        ViewBag.Sedes = (await _sedes.ListarAsync(soloActivas: true)).ToList();
        ViewBag.Trabajadores = (await _trabajadores.ListarAsync(soloActivos: true)).ToList();
    }
}
