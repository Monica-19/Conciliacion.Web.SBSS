using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public interface ITrabajadorRepository
{
    Task<int> InsertarAsync(CrearTrabajadorVm vm);
    Task ActualizarAsync(EditarTrabajadorVm vm);
    Task CambiarEstadoAsync(int idTrabajador, string estadoTrabajador);
    Task<Trabajador?> ObtenerPorIdAsync(int idTrabajador);
    Task<IEnumerable<Trabajador>> ListarAsync(int? idSedeRef = null, bool soloActivos = false);
}
