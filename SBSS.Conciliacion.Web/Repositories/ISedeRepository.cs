using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public interface ISedeRepository
{
    Task<int> InsertarAsync(CrearSedeVm vm);
    Task ActualizarAsync(EditarSedeVm vm);
    Task CambiarEstadoAsync(int idSede, string estadoSede);
    Task<Sede?> ObtenerPorIdAsync(int idSede);
    Task<IEnumerable<Sede>> ListarAsync(bool soloActivas = false);
}
