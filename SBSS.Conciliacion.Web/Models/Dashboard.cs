namespace SBSS.Conciliacion.Web.Models;

/// <summary>sp_Dashboard_ResumenGeneral</summary>
public class DashboardResumenVm
{
    public int TotalExpedientes { get; set; }
    public int Registrados { get; set; }
    public int Asignados { get; set; }
    public int EnProceso { get; set; }
    public int Concluidos { get; set; }
    public int Archivados { get; set; }
    public int Vencidos { get; set; }
}

/// <summary>sp_Dashboard_ExpedientesPorMes</summary>
public class ExpedientesPorMesVm
{
    public int Mes { get; set; }
    public int Cantidad { get; set; }
}

/// <summary>sp_Dashboard_ExpedientesPorConciliador</summary>
public class ExpedientesPorConciliadorVm
{
    public string Conciliador { get; set; } = string.Empty;
    public int TotalAsignados { get; set; }
    public int Concluidos { get; set; }
}

/// <summary>sp_Dashboard_TasaExitoConciliacion</summary>
public class TasaExitoVm
{
    public string Resultado { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class DashboardViewModel
{
    public DashboardResumenVm Resumen { get; set; } = new();
    public List<ExpedientesPorMesVm> PorMes { get; set; } = new();
    public List<ExpedientesPorConciliadorVm> PorConciliador { get; set; } = new();
    public List<TasaExitoVm> TasaExito { get; set; } = new();
}
