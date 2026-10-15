using System.Data;
using Dapper;
using SBSS.Conciliacion.Web.Data;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly IDbConnectionFactory _factory;

    public UsuarioRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<UsuarioLogin?> ObtenerParaLoginAsync(string usuario)
    {
        using var conn = _factory.CreateConnection();
        // sp_Usuario_ObtenerParaLogin solo trae el hash: la comparación se hace en la app (BCrypt),
        // nunca comparando contraseña en texto plano dentro del SP.
        return await conn.QueryFirstOrDefaultAsync<UsuarioLogin>(
            "sp_Usuario_ObtenerParaLogin",
            new { usuario },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int?> ObtenerIdUsuarioPorTrabajadorAsync(int idTrabajador)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<int?>(
            "sp_Usuario_ObtenerIdPorTrabajador",
            new { idTrabajador },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> InsertarAsync(int idTrabajador, string usuario, string contraseniaHash, string rol)
    {
        using var conn = _factory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@idTrabajador", idTrabajador);
        parametros.Add("@usuario", usuario);
        parametros.Add("@contraseniaHash", contraseniaHash);
        parametros.Add("@rol", rol);
        parametros.Add("@idUsuarioNuevo", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync("sp_Usuario_Insertar", parametros, commandType: CommandType.StoredProcedure);
        return parametros.Get<int>("@idUsuarioNuevo");
    }

    public async Task ActualizarContraseniaAsync(int idUsuario, string contraseniaHash)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_Usuario_ActualizarContrasenia",
            new { idUsuario, contraseniaHash },
            commandType: CommandType.StoredProcedure);
    }

    public async Task GenerarTokenResetAsync(int idUsuario, string token, int minutosValidez)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_PasswordResetToken_Generar",
            new { idUsuario, token, minutosValidez },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int?> ValidarTokenResetAsync(string token)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<int?>(
            "sp_PasswordResetToken_Validar",
            new { token },
            commandType: CommandType.StoredProcedure);
    }

    public async Task MarcarTokenResetUsadoAsync(string token)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_PasswordResetToken_MarcarUsado",
            new { token },
            commandType: CommandType.StoredProcedure);
    }
}
