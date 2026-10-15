using System.Data;
using Dapper;
using SBSS.Conciliacion.Web.Data;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly IDbConnectionFactory _factory;

    public DashboardRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<DashboardResumenVm> ObtenerResumenAsync()
    {
        using var conn = _factory.CreateConnection();
        var resumen = await conn.QueryFirstOrDefaultAsync<DashboardResumenVm>(
            "sp_Dashboard_ResumenGeneral",
            commandType: CommandType.StoredProcedure);
        return resumen ?? new DashboardResumenVm();
    }

    public async Task<IEnumerable<ExpedientesPorMesVm>> ObtenerPorMesAsync(int anio)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<ExpedientesPorMesVm>(
            "sp_Dashboard_ExpedientesPorMes",
            new { anio },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<ExpedientesPorConciliadorVm>> ObtenerPorConciliadorAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<ExpedientesPorConciliadorVm>(
            "sp_Dashboard_ExpedientesPorConciliador",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<TasaExitoVm>> ObtenerTasaExitoAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<TasaExitoVm>(
            "sp_Dashboard_TasaExitoConciliacion",
            commandType: CommandType.StoredProcedure);
    }
}
