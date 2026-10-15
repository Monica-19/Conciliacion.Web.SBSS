using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public interface IUsuarioRepository
{
    Task<UsuarioLogin?> ObtenerParaLoginAsync(string usuario);

    /// <summary>Resuelve el idUsuario (credenciales) de un trabajador, o null si aún no tiene acceso al sistema.</summary>
    Task<int?> ObtenerIdUsuarioPorTrabajadorAsync(int idTrabajador);

    /// <summary>Crea las credenciales de acceso de un trabajador. El hash BCrypt ya viene calculado.</summary>
    Task<int> InsertarAsync(int idTrabajador, string usuario, string contraseniaHash, string rol);

    /// <summary>Actualiza la contraseña (ya hasheada) de un usuario.</summary>
    Task ActualizarContraseniaAsync(int idUsuario, string contraseniaHash);

    // ---- Recuperación de contraseña ----
    Task GenerarTokenResetAsync(int idUsuario, string token, int minutosValidez);
    Task<int?> ValidarTokenResetAsync(string token);
    Task MarcarTokenResetUsadoAsync(string token);
}
