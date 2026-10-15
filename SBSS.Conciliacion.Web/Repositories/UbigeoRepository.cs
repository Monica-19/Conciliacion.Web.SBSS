using System.Data;
using Dapper;
using SBSS.Conciliacion.Web.Data;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public class UbigeoRepository : IUbigeoRepository
{
    private readonly IDbConnectionFactory _factory;

    public UbigeoRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<Departamento>> ListarDepartamentosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<Departamento>(
            "sp_Departamento_Listar",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Provincia>> ListarProvinciasAsync(int idDepartamento)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<Provincia>(
            "sp_Provincia_ListarPorDepartamento",
            new { idDepartamento },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Distrito>> ListarDistritosAsync(int idProvincia)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<Distrito>(
            "sp_Distrito_ListarPorProvincia",
            new { idProvincia },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<(int IdDepartamento, int IdProvincia, int IdDistrito)?> BuscarDistritoPorNombresAsync(
        string nombreDistrito, string nombreProvincia, string nombreDepartamento)
    {
        using var conn = _factory.CreateConnection();
        var fila = await conn.QueryFirstOrDefaultAsync<DistritoSugeridoRow>(
            "sp_Distrito_BuscarPorNombres",
            new { nombreDistrito, nombreProvincia, nombreDepartamento },
            commandType: CommandType.StoredProcedure);

        if (fila is null) return null;

        return (fila.IdDepartamento, fila.IdProvincia, fila.IdDistrito);
    }

    private class DistritoSugeridoRow
    {
        public int IdDepartamento { get; set; }
        public int IdProvincia { get; set; }
        public int IdDistrito { get; set; }
    }
}
