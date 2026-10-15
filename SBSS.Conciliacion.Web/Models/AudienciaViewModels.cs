using System.ComponentModel.DataAnnotations;

namespace SBSS.Conciliacion.Web.Models;

/// <summary>Formulario para programar una audiencia (sp_Audiencia_Programar).</summary>
public class ProgramarAudienciaVm
{
    [Required]
    [Display(Name = "Expediente")]
    public int IdExpediente { get; set; }

    public string? NumeroExpediente { get; set; }

    [Required]
    [Display(Name = "Tipo de documento (citación / acta)")]
    public int IdTipoDocumentos { get; set; }

    [Required(ErrorMessage = "Selecciona un conciliador.")]
    [Display(Name = "Conciliador")]
    public int IdConciliador { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de audiencia")]
    public DateTime Fecha { get; set; } = DateTime.Today.AddDays(3);

    [Required]
    [DataType(DataType.Time)]
    [Display(Name = "Hora")]
    public TimeSpan Hora { get; set; } = new TimeSpan(9, 0, 0);

    [Range(1, 20)]
    [Display(Name = "N.° de sesión")]
    public int NumeroSesion { get; set; } = 1;
}

public class ReprogramarAudienciaVm
{
    [Required]
    public int IdDocumento { get; set; }

    [Required]
    public int IdExpediente { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Nueva fecha")]
    public DateTime NuevaFecha { get; set; }

    [Required]
    [DataType(DataType.Time)]
    [Display(Name = "Nueva hora")]
    public TimeSpan NuevaHora { get; set; }

    [Required(ErrorMessage = "Indica el motivo de la reprogramación.")]
    [Display(Name = "Motivo")]
    public string Motivo { get; set; } = string.Empty;
}

public class RegistrarResultadoVm
{
    [Required]
    public int IdDocumento { get; set; }

    public int IdExpediente { get; set; }

    [Required(ErrorMessage = "Selecciona el resultado de la audiencia.")]
    [Display(Name = "Resultado")]
    public string Resultado { get; set; } = string.Empty;

    [Display(Name = "Observaciones")]
    [DataType(DataType.MultilineText)]
    public string? Observaciones { get; set; }
}

/// <summary>Valores válidos para Audiencia.resultado (según BD-SBSS-Completa.sql).</summary>
public static class ResultadosAudiencia
{
    public const string AcuerdoTotal = "ACUERDO_TOTAL";
    public const string AcuerdoParcial = "ACUERDO_PARCIAL";
    public const string NoAcuerdo = "NO_ACUERDO";
    public const string InasistenciaAmbas = "INASISTENCIA_AMBAS";
    public const string InasistenciaUna = "INASISTENCIA_UNA";
    public const string Reprogramada = "REPROGRAMADA";

    public static readonly Dictionary<string, string> Etiquetas = new()
    {
        [AcuerdoTotal] = "Acuerdo total",
        [AcuerdoParcial] = "Acuerdo parcial",
        [NoAcuerdo] = "No hubo acuerdo",
        [InasistenciaAmbas] = "Inasistencia de ambas partes",
        [InasistenciaUna] = "Inasistencia de una parte",
        [Reprogramada] = "Reprogramada"
    };
}
