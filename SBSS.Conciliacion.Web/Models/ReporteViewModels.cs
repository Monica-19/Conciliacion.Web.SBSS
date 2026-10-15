using System.ComponentModel.DataAnnotations;

namespace SBSS.Conciliacion.Web.Models;

/// <summary>Fila de sp_Reporte_ExpedientesPorRango.</summary>
public class ReporteExpedienteVm
{
    public string NumeroExpediente { get; set; } = string.Empty;
    public string? EstadoProceso { get; set; }
    public DateTime? FechaRecepcion { get; set; }
    public DateTime? FechaNotificacion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public DateTime? FechaCierre { get; set; }
    public string? Conciliador { get; set; }
    public string? Sede { get; set; }
}

/// <summary>Fila de sp_Reporte_ExpedientesPorMateria.</summary>
public class ReporteMateriaVm
{
    public string NombreTipoMateria { get; set; } = string.Empty;
    public string NombreMateria { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class FiltroReporteRangoVm
{
    [Required(ErrorMessage = "Indica la fecha de inicio.")]
    [Display(Name = "Desde")]
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; } = DateTime.Today.AddMonths(-1);

    [Required(ErrorMessage = "Indica la fecha de fin.")]
    [Display(Name = "Hasta")]
    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; } = DateTime.Today;
}
