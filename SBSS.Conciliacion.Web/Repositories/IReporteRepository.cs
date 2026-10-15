using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public interface IReporteRepository
{
    Task<IEnumerable<ReporteExpedienteVm>> ObtenerPorRangoAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<IEnumerable<ReporteMateriaVm>> ObtenerPorMateriaAsync(int? idTipoMateria);
}
