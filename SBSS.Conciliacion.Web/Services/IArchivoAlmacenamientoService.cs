using Microsoft.AspNetCore.Http;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Services;

public record ResultadoGuardarArchivo(bool Exito, string? Error, string? RutaRelativa, int TamanioKB);

/// <summary>
/// Guarda archivos/videos adjuntos a un expediente en disco, validando
/// extensión y tamaño según el tipo declarado (ver TiposArchivoAdjunto).
/// Nunca confía en el nombre original del archivo para la ruta física:
/// genera un nombre aleatorio y conserva el nombre original solo para
/// mostrarlo en pantalla (columna nombreArchivo en la BD).
/// </summary>
public interface IArchivoAlmacenamientoService
{
    Task<ResultadoGuardarArchivo> GuardarAsync(int idExpediente, string tipoArchivo, IFormFile archivo);
    string ObtenerRutaFisica(string rutaRelativa);
}
