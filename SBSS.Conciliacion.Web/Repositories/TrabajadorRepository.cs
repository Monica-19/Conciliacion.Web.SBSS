using System.Data;
using Dapper;
using SBSS.Conciliacion.Web.Data;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public class TrabajadorRepository : ITrabajadorRepository
{
    private readonly IDbConnectionFactory _factory;

    public TrabajadorRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<int> InsertarAsync(CrearTrabajadorVm vm)
    {
        using var conn = _factory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@idSedeRef", vm.IdSedeRef);
        parametros.Add("@dniTrabajador", vm.DniTrabajador);
        parametros.Add("@nombreTrabajador", vm.NombreTrabajador);
        parametros.Add("@apePaterTrabajador", vm.ApePaterTrabajador);
        parametros.Add("@apeMaterTrabajador", vm.ApeMaterTrabajador);
        parametros.Add("@emailTrabajador", vm.EmailTrabajador);
        parametros.Add("@telefonoTrabajador", vm.TelefonoTrabajador);
        parametros.Add("@registroCivilComercial", vm.RegistroCivilComercial);
        parametros.Add("@idTrabajadorNuevo", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync("sp_Trabajador_Insertar", parametros, commandType: CommandType.StoredProcedure);
        return parametros.Get<int>("@idTrabajadorNuevo");
    }

    public async Task ActualizarAsync(EditarTrabajadorVm vm)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_Trabajador_Actualizar",
            new
            {
                idTrabajador = vm.IdTrabajador,
                idSedeRef = vm.IdSedeRef,
                nombreTrabajador = vm.NombreTrabajador,
                apePaterTrabajador = vm.ApePaterTrabajador,
                apeMaterTrabajador = vm.ApeMaterTrabajador,
                emailTrabajador = vm.EmailTrabajador,
                telefonoTrabajador = vm.TelefonoTrabajador
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task CambiarEstadoAsync(int idTrabajador, string estadoTrabajador)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_Trabajador_CambiarEstado",
            new { idTrabajador, estadoTrabajador },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Trabajador?> ObtenerPorIdAsync(int idTrabajador)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Trabajador>(
            "sp_Trabajador_ObtenerPorId",
            new { idTrabajador },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Trabajador>> ListarAsync(int? idSedeRef = null, bool soloActivos = false)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<Trabajador>(
            "sp_Trabajador_Listar",
            new { idSedeRef, soloActivos },
            commandType: CommandType.StoredProcedure);
    }
}
