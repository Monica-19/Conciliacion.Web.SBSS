using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Services;

public interface IReportePdfService
{
    byte[] GenerarReporteExpedientes(IEnumerable<ExpedienteListItemVm> expedientes);
    byte[] GenerarReportePorRango(IEnumerable<ReporteExpedienteVm> expedientes);
    byte[] GenerarReportePorMateria(IEnumerable<ReporteMateriaVm> materias);
}
