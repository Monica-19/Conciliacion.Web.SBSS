using System.Data;
using Dapper;
using SBSS.Conciliacion.Web.Data;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public class SedeRepository : ISedeRepository
{
    private readonly IDbConnectionFactory _factory;

    public SedeRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<int> InsertarAsync(CrearSedeVm vm)
    {
        using var conn = _factory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@nombre", vm.Nombre);
        parametros.Add("@descripcion", vm.Descripcion);
        parametros.Add("@idSedeNuevo", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync("sp_Sede_Insertar", parametros, commandType: CommandType.StoredProcedure);
        return parametros.Get<int>("@idSedeNuevo");
    }

    public async Task ActualizarAsync(EditarSedeVm vm)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_Sede_Actualizar",
            new { idSede = vm.IdSede, nombre = vm.Nombre, descripcion = vm.Descripcion },
            commandType: CommandType.StoredProcedure);
    }

    public async Task CambiarEstadoAsync(int idSede, string estadoSede)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_Sede_CambiarEstado",
            new { idSede, estadoSede },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Sede?> ObtenerPorIdAsync(int idSede)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Sede>(
            "sp_Sede_ObtenerPorId",
            new { idSede },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Sede>> ListarAsync(bool soloActivas = false)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<Sede>(
            "sp_Sede_Listar",
            new { soloActivas },
            commandType: CommandType.StoredProcedure);
    }
}
