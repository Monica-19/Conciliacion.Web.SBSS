using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public interface IUbigeoRepository
{
    Task<IEnumerable<Departamento>> ListarDepartamentosAsync();
    Task<IEnumerable<Provincia>> ListarProvinciasAsync(int idDepartamento);
    Task<IEnumerable<Distrito>> ListarDistritosAsync(int idProvincia);

    /// <summary>Intenta emparejar distrito/provincia/departamento (texto) contra el catálogo. Null si no hay coincidencia.</summary>
    Task<(int IdDepartamento, int IdProvincia, int IdDistrito)?> BuscarDistritoPorNombresAsync(
        string nombreDistrito, string nombreProvincia, string nombreDepartamento);
}
