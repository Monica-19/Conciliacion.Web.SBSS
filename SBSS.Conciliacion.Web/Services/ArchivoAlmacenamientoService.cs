using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Services;

public class ArchivoAlmacenamientoService : IArchivoAlmacenamientoService
{
    private readonly string _carpetaBase;

    public ArchivoAlmacenamientoService(IWebHostEnvironment env, IConfiguration config)
    {
        var carpetaConfigurada = config["Almacenamiento:CarpetaArchivos"] ?? "App_Data/uploads";
        _carpetaBase = Path.Combine(env.ContentRootPath, carpetaConfigurada);
    }

    public async Task<ResultadoGuardarArchivo> GuardarAsync(int idExpediente, string tipoArchivo, IFormFile archivo)
    {
        if (!TiposArchivoAdjunto.ExtensionesPermitidas.ContainsKey(tipoArchivo))
            return new ResultadoGuardarArchivo(false, "Tipo de archivo no reconocido.", null, 0);

        if (archivo is null || archivo.Length == 0)
            return new ResultadoGuardarArchivo(false, "Selecciona un archivo.", null, 0);

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var extensionesValidas = TiposArchivoAdjunto.ExtensionesPermitidas[tipoArchivo];
        if (!extensionesValidas.Contains(extension))
        {
            return new ResultadoGuardarArchivo(
                false,
                $"Extensión no permitida para {TiposArchivoAdjunto.Etiquetas[tipoArchivo]}. Formatos válidos: {string.Join(", ", extensionesValidas)}.",
                null, 0);
        }

        var tamanioMaximo = TiposArchivoAdjunto.TamanioMaximoBytes[tipoArchivo];
        if (archivo.Length > tamanioMaximo)
        {
            return new ResultadoGuardarArchivo(
                false,
                $"El archivo supera el tamaño máximo permitido ({tamanioMaximo / (1024 * 1024)} MB).",
                null, 0);
        }

        // Nombre físico aleatorio: nunca se usa el nombre original en el disco,
        // así se evita path traversal y colisiones de nombres.
        var nombreFisico = $"{Guid.NewGuid():N}{extension}";
        var carpetaExpediente = Path.Combine(_carpetaBase, idExpediente.ToString());
        Directory.CreateDirectory(carpetaExpediente);

        var rutaFisica = Path.Combine(carpetaExpediente, nombreFisico);
        await using (var destino = File.Create(rutaFisica))
        {
            await archivo.CopyToAsync(destino);
        }

        var rutaRelativa = Path.Combine(idExpediente.ToString(), nombreFisico).Replace('\\', '/');
        var tamanioKB = (int)Math.Ceiling(archivo.Length / 1024.0);

        return new ResultadoGuardarArchivo(true, null, rutaRelativa, tamanioKB);
    }

    public string ObtenerRutaFisica(string rutaRelativa) => Path.Combine(_carpetaBase, rutaRelativa);
}
