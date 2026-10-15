using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public interface IDashboardRepository
{
    Task<DashboardResumenVm> ObtenerResumenAsync();
    Task<IEnumerable<ExpedientesPorMesVm>> ObtenerPorMesAsync(int anio);
    Task<IEnumerable<ExpedientesPorConciliadorVm>> ObtenerPorConciliadorAsync();
    Task<IEnumerable<TasaExitoVm>> ObtenerTasaExitoAsync();
}
