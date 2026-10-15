using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SBSS.Conciliacion.Web.Models;

public class DocumentoGenerado
{
    public int IdDocumento { get; set; }
    public int IdTipoDocumentos { get; set; }
    public int IdExpediente { get; set; }
    public DateTime? FechaGenerado { get; set; }
}

/// <summary>Catálogo de tipos de documento (Documentos.idTipoDocumentos / nombre).</summary>
public class TipoDocumento
{
    public int IdTipoDocumentos { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>Formulario de subida de un archivo o video a un expediente.</summary>
public class SubirArchivoVm
{
    [Required]
    public int IdExpediente { get; set; }

    [Required(ErrorMessage = "Selecciona el tipo de archivo.")]
    [Display(Name = "Tipo")]
    public string TipoArchivo { get; set; } = TiposArchivoAdjunto.Documento;

    [Required(ErrorMessage = "Selecciona un archivo.")]
    [Display(Name = "Archivo")]
    public IFormFile Archivo { get; set; } = default!;
}

/// <summary>Extensiones permitidas y tamaño máximo por tipo de archivo adjunto.</summary>
public static class TiposArchivoAdjunto
{
    public const string Documento = "DOCUMENTO";
    public const string Imagen = "IMAGEN";
    public const string Video = "VIDEO";
    public const string Audio = "AUDIO";

    public static readonly Dictionary<string, string[]> ExtensionesPermitidas = new()
    {
        [Documento] = new[] { ".pdf", ".doc", ".docx" },
        [Imagen] = new[] { ".jpg", ".jpeg", ".png", ".webp" },
        [Video] = new[] { ".mp4", ".mov", ".avi", ".webm" },
        [Audio] = new[] { ".mp3", ".wav", ".m4a" }
    };

    public static readonly Dictionary<string, long> TamanioMaximoBytes = new()
    {
        [Documento] = 15L * 1024 * 1024,   // 15 MB
        [Imagen] = 15L * 1024 * 1024,      // 15 MB
        [Video] = 200L * 1024 * 1024,      // 200 MB
        [Audio] = 50L * 1024 * 1024        // 50 MB
    };

    public static readonly Dictionary<string, string> Etiquetas = new()
    {
        [Documento] = "Documento",
        [Imagen] = "Imagen",
        [Video] = "Video",
        [Audio] = "Audio"
    };
}

/// <summary>Formulario para registrar el acta de conciliación de una audiencia (sp_ActaConciliacion_Registrar).</summary>
public class ActaConciliacionVm
{
    [Required]
    public int IdDocumento { get; set; }

    public int IdExpediente { get; set; }

    [Required(ErrorMessage = "Describe la controversia.")]
    [Display(Name = "Descripción de la controversia")]
    [DataType(DataType.MultilineText)]
    public string DescripcionControversia { get; set; } = string.Empty;

    [Required(ErrorMessage = "Describe el acuerdo conciliatorio.")]
    [Display(Name = "Acuerdo conciliatorio")]
    [DataType(DataType.MultilineText)]
    public string AcuerdoConciliatorio { get; set; } = string.Empty;
}

public class ArchivoAdjuntoVm
{
    public int IdArchivo { get; set; }
    public int IdExpediente { get; set; }
    public string TipoArchivo { get; set; } = string.Empty; // DOCUMENTO, VIDEO, IMAGEN, AUDIO
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public int? TamanioKB { get; set; }
    public DateTime FechaSubida { get; set; }
}
