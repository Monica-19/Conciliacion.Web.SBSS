using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public interface IPersonaRepository
{
    Task<int> InsertarNaturalAsync(CrearPersonaNaturalVm vm);
    Task<int> InsertarJuridicaAsync(CrearPersonaJuridicaVm vm);
    Task<PersonaDetalle?> ObtenerPorIdAsync(int idPersona);
    Task<IEnumerable<PersonaListItem>> ListarAsync(string? busqueda = null);
    Task CambiarEstadoAsync(int idPersona, string estadoPersona);

    // ---- Dirección (opcional) ----
    Task<int> InsertarDireccionAsync(DireccionVm direccion);
    Task VincularDireccionAsync(int idPersona, int idDireccion, bool esPrincipal = true);
    Task<DireccionDetalleVm?> ObtenerDireccionPrincipalAsync(int idPersona);
}
