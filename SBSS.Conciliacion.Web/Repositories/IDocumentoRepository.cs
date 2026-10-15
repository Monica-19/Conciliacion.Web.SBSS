using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public interface IDocumentoRepository
{
    Task<int> InsertarDocumentoGeneradoAsync(int idTipoDocumentos, int idExpediente);
    Task RegistrarActaConciliacionAsync(ActaConciliacionVm vm);
    Task RegistrarActaNotificacionAsync(int idDocumento, DateTime fechaNotificacion, TimeSpan horaNotificacion);
    Task InsertarAnexoAsync(int idDocumento, string nombreArchivo, string rutaArchivo);
    Task InsertarArchivoAdjuntoAsync(int idExpediente, int idTrabajador, string tipoArchivo, string nombreArchivo, string rutaArchivo, int? tamanioKB);
    Task<IEnumerable<ArchivoAdjuntoVm>> ListarArchivosPorExpedienteAsync(int idExpediente, string? tipoArchivo = null);
    Task<ArchivoAdjuntoVm?> ObtenerArchivoPorIdAsync(int idArchivo);
    Task<IEnumerable<TipoDocumento>> ListarTiposDocumentoAsync();
}
