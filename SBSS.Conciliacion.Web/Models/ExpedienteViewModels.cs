using System.ComponentModel.DataAnnotations;

namespace SBSS.Conciliacion.Web.Models;

/// <summary>Fila de la grilla de expedientes (sp_Expediente_Listar).</summary>
public class ExpedienteListItemVm
{
    public int IdExpediente { get; set; }
    public string NumeroExpediente { get; set; } = string.Empty;
    public string? EstadoProceso { get; set; }
    public DateTime? FechaRecepcion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public string? Conciliador { get; set; }


    public bool EstaVencido =>
        FechaVencimiento.HasValue
        && FechaVencimiento.Value < DateTime.Now
        && EstadoProceso is not ("CONCLUIDO" or "ARCHIVADO");
}

/// <summary>Filtros de búsqueda para Expedientes/Index.</summary>
public class ExpedienteFiltroVm
{
    public string? Estado { get; set; }
    public int? IdConciliador { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanioPagina { get; set; } = 20;
    public int? IdMateria { get; set; }

}

/// <summary>Formulario de registro de un nuevo expediente (sp_Expediente_Insertar).</summary>
public class CrearExpedienteVm
{
    [Required(ErrorMessage = "El número de expediente es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Número de expediente")]
    public string NumeroExpediente { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Trabajador que recibe")]
    public int IdTrabajador { get; set; }

    [Required]
    [Display(Name = "Sede")]
    public int IdSede { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de recepción")]
    public DateTime FechaRecepcion { get; set; } = DateTime.Today;

    [Range(1, 90)]
    [Display(Name = "Plazo (días hábiles)")]
    public int PlazoDias { get; set; } = 30;
}

/// <summary>Vista de detalle: expediente + personas + materias (3 result sets de sp_Expediente_ObtenerPorId).</summary>
public class ExpedienteDetalleVm
{
    public Expediente Expediente { get; set; } = new();
    public List<PersonaExpedienteVm> Personas { get; set; } = new();
    public List<MateriaExpedienteVm> Materias { get; set; } = new();
    public List<AudienciaListItemVm> Audiencias { get; set; } = new();
    public List<ArchivoAdjuntoVm> Archivos { get; set; } = new();
}

public class PersonaExpedienteVm
{
    public int IdPersona { get; set; }
    public string? RolParticipante { get; set; }
    public string? CorreoP { get; set; }
    public string? CelularP { get; set; }
}

public class MateriaExpedienteVm
{
    public string NombreMateria { get; set; } = string.Empty;
    public string? DescripcionDtMateria { get; set; }
}

public class CambiarEstadoVm
{
    [Required]
    public int IdExpediente { get; set; }

    [Required]
    [Display(Name = "Nuevo estado")]
    public string NuevoEstado { get; set; } = string.Empty;
}
