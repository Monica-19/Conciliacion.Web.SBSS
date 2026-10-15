namespace SBSS.Conciliacion.Web.Models;

public class Audiencia
{
    public int IdDocumento { get; set; }
    public int? IdConciliador { get; set; }
    public int NumeroSesion { get; set; } = 1;
    public DateTime? Fecha { get; set; }
    public TimeSpan? Hora { get; set; }
    public string? Estado { get; set; }
    public string? Resultado { get; set; }
    public string? Observaciones { get; set; }
}

public class AudienciaListItemVm
{
    public int IdDocumento { get; set; }
    public int NumeroSesion { get; set; }
    public DateTime? Fecha { get; set; }
    public TimeSpan? Hora { get; set; }
    public string? Estado { get; set; }
    public string? Resultado { get; set; }
    public string? Observaciones { get; set; }
    public string? Conciliador { get; set; }
}

/// <summary>Resultado de sp_Audiencia_ObtenerPorId — vista de detalle completo de una audiencia.</summary>
public class AudienciaDetalleVm
{
    public int IdDocumento { get; set; }
    public int NumeroSesion { get; set; }
    public DateTime? Fecha { get; set; }
    public TimeSpan? Hora { get; set; }
    public string? Estado { get; set; }
    public string? Resultado { get; set; }
    public string? Observaciones { get; set; }
    public int IdExpediente { get; set; }
    public string NumeroExpediente { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string? Conciliador { get; set; }
}
