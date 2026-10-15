using System.Data;
using Dapper;
using SBSS.Conciliacion.Web.Data;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public class AudienciaRepository : IAudienciaRepository
{
    private readonly IDbConnectionFactory _factory;

    public AudienciaRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<int> ProgramarAsync(ProgramarAudienciaVm vm)
    {
        using var conn = _factory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@idExpediente", vm.IdExpediente);
        parametros.Add("@idTipoDocumentos", vm.IdTipoDocumentos);
        parametros.Add("@idConciliador", vm.IdConciliador);
        parametros.Add("@fecha", vm.Fecha);
        parametros.Add("@hora", vm.Hora);
        parametros.Add("@numeroSesion", vm.NumeroSesion);
        parametros.Add("@idAudiencia", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync(
            "sp_Audiencia_Programar",
            parametros,
            commandType: CommandType.StoredProcedure);

        return parametros.Get<int>("@idAudiencia");
    }

    public async Task ReprogramarAsync(ReprogramarAudienciaVm vm)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_Audiencia_Reprogramar",
            new
            {
                idDocumento = vm.IdDocumento,
                nuevaFecha = vm.NuevaFecha,
                nuevaHora = vm.NuevaHora,
                motivo = vm.Motivo
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task RegistrarResultadoAsync(RegistrarResultadoVm vm)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_Audiencia_RegistrarResultado",
            new
            {
                idDocumento = vm.IdDocumento,
                resultado = vm.Resultado,
                observaciones = vm.Observaciones
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<AudienciaListItemVm>> ListarPorExpedienteAsync(int idExpediente)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<AudienciaListItemVm>(
            "sp_Audiencia_ListarPorExpediente",
            new { idExpediente },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<AudienciaDetalleVm?> ObtenerPorIdAsync(int idDocumento)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<AudienciaDetalleVm>(
            "sp_Audiencia_ObtenerPorId",
            new { idDocumento },
            commandType: CommandType.StoredProcedure);
    }
}
