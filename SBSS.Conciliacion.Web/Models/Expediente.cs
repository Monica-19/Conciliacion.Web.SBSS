namespace SBSS.Conciliacion.Web.Models;

public class Expediente
{
    public int IdExpediente { get; set; }
    public int? IdConciliador { get; set; }
    public int? IdTrabajador { get; set; }
    public int? IdSede { get; set; }
    public string NumeroExpediente { get; set; } = string.Empty;
    public string? EstadoProceso { get; set; }
    public DateTime? FechaRecepcion { get; set; }
    public DateTime? FechaNotificacion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public DateTime? FechaCierre { get; set; }

    // columnas calculadas que trae sp_Expediente_ObtenerPorId / sp_Expediente_Listar
    public string? NombreConciliador { get; set; }
    public string? NombreSede { get; set; }
    public string? Conciliador { get; set; }

    public bool EstaVencido =>
        FechaVencimiento.HasValue
        && FechaVencimiento.Value < DateTime.Now
        && EstadoProceso is not ("CONCLUIDO" or "ARCHIVADO");
}
