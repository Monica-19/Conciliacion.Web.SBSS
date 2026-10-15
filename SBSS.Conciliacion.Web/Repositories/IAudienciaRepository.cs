using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public interface IAudienciaRepository
{
    Task<int> ProgramarAsync(ProgramarAudienciaVm vm);
    Task ReprogramarAsync(ReprogramarAudienciaVm vm);
    Task RegistrarResultadoAsync(RegistrarResultadoVm vm);
    Task<IEnumerable<AudienciaListItemVm>> ListarPorExpedienteAsync(int idExpediente);
    Task<AudienciaDetalleVm?> ObtenerPorIdAsync(int idDocumento);
}
