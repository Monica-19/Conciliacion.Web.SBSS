using Dapper;
using DocumentFormat.OpenXml.InkML;
using SBSS.Conciliacion.Web.Data;
using SBSS.Conciliacion.Web.Models;
using System.Data;

namespace SBSS.Conciliacion.Web.Repositories;

public class MateriaRepository : IMateriaRepository
{
    private readonly IDbConnectionFactory _factory;

    public MateriaRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<int> InsertarTipoAsync(CrearTipoMateriaVm vm)
    {
        using var conn = _factory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@nombreTipoMateria", vm.NombreTipoMateria);
        parametros.Add("@descripcionTipoMateria", vm.DescripcionTipoMateria);
        parametros.Add("@idTipoMateriaNuevo", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync("sp_TipoMateria_Insertar", parametros, commandType: CommandType.StoredProcedure);
        return parametros.Get<int>("@idTipoMateriaNuevo");
    }

    public async Task<IEnumerable<TipoMateria>> ListarTiposAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<TipoMateria>(
            "sp_TipoMateria_Listar",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> InsertarMateriaAsync(CrearMateriaVm vm)
    {
        using var conn = _factory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@idTipoMateria", vm.IdTipoMateria);
        parametros.Add("@nombreMateria", vm.NombreMateria);
        parametros.Add("@descripcionMateria", vm.DescripcionMateria);
        parametros.Add("@idMateriaNuevo", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync("sp_Materia_Insertar", parametros, commandType: CommandType.StoredProcedure);
        return parametros.Get<int>("@idMateriaNuevo");
    }

    public async Task ActualizarMateriaAsync(EditarMateriaVm vm)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_Materia_Actualizar",
            new
            {
                idMateria = vm.IdMateria,
                idTipoMateria = vm.IdTipoMateria,
                nombreMateria = vm.NombreMateria,
                descripcionMateria = vm.DescripcionMateria
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Materia?> ObtenerMateriaPorIdAsync(int idMateria)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Materia>(
            "sp_Materia_ObtenerPorId",
            new { idMateria },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Materia>> ListarMateriasAsync(int? idTipoMateria = null)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<Materia>(
            "sp_Materia_Listar",
            new { idTipoMateria },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> EliminarMateriaAsync(int id)
    {
        using var connection = _factory.CreateConnection(); 

        var parametros = new DynamicParameters();
        parametros.Add("@IdMateria", id);
        parametros.Add("@Respuesta", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        parametros.Add("@Mensaje", dbType: DbType.String, size: 250, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("sp_EliminarMateria", parametros, commandType: CommandType.StoredProcedure);

        return parametros.Get<bool>("@Respuesta");
    }
}
