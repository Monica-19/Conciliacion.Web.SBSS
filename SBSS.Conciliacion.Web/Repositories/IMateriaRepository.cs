using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public interface IMateriaRepository
{
    Task<int> InsertarTipoAsync(CrearTipoMateriaVm vm);
    Task<IEnumerable<TipoMateria>> ListarTiposAsync();

    Task<int> InsertarMateriaAsync(CrearMateriaVm vm);
    Task ActualizarMateriaAsync(EditarMateriaVm vm);
    Task<Materia?> ObtenerMateriaPorIdAsync(int idMateria);
    Task<IEnumerable<Materia>> ListarMateriasAsync(int? idTipoMateria = null);
    Task<bool> EliminarMateriaAsync(int idMateria);
}
