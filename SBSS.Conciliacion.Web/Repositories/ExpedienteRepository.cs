using System.Data;
using Dapper;
using SBSS.Conciliacion.Web.Data;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;


public class ExpedienteRepository : IExpedienteRepository
{
    private readonly IDbConnectionFactory _factory;

    public ExpedienteRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<int> InsertarAsync(CrearExpedienteVm vm)
    {
        using var conn = _factory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@numeroExpediente", vm.NumeroExpediente);
        parametros.Add("@idTrabajador", vm.IdTrabajador);
        parametros.Add("@idSede", vm.IdSede);
        parametros.Add("@fechaRecepcion", vm.FechaRecepcion);
        parametros.Add("@plazoDias", vm.PlazoDias);
        parametros.Add("@idExpedienteNuevo", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync(
            "sp_Expediente_Insertar",
            parametros,
            commandType: CommandType.StoredProcedure);

        return parametros.Get<int>("@idExpedienteNuevo");
    }

    public async Task<int> AsignarConciliadorAleatorioAsync(int idExpediente)
    {
        using var conn = _factory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@idExpediente", idExpediente);
        parametros.Add("@idConciliadorAsignado", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync("sp_Expediente_AsignarConciliadorAleatorio", parametros, commandType: CommandType.StoredProcedure);
        return parametros.Get<int>("@idConciliadorAsignado");
    }

    public async Task CambiarEstadoAsync(int idExpediente, string nuevoEstado)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_Expediente_CambiarEstado",
            new { idExpediente, nuevoEstado },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<ExpedienteDetalleVm?> ObtenerPorIdAsync(int idExpediente)
    {
        using var conn = _factory.CreateConnection();

        using var multi = await conn.QueryMultipleAsync(
            "sp_Expediente_ObtenerPorId",
            new { idExpediente },
            commandType: CommandType.StoredProcedure);

        var expediente = await multi.ReadFirstOrDefaultAsync<Expediente>();
        if (expediente is null) return null;

        var personas = (await multi.ReadAsync<PersonaExpedienteVm>()).ToList();
        var materias = (await multi.ReadAsync<MateriaExpedienteVm>()).ToList();

        return new ExpedienteDetalleVm
        {
            Expediente = expediente,
            Personas = personas,
            Materias = materias
        };
    }

    public async Task<IEnumerable<ExpedienteListItemVm>> ListarAsync(ExpedienteFiltroVm filtro)
    {
        using var conn = _factory.CreateConnection();

        return await conn.QueryAsync<ExpedienteListItemVm>(
            "sp_Expediente_Listar",
            new
            {
                estado = filtro.Estado,
                idConciliador = filtro.IdConciliador,
                fechaInicio = filtro.FechaInicio,
                fechaFin = filtro.FechaFin,
                pagina = filtro.Pagina,
                tamanioPagina = filtro.TamanioPagina,
                idMateria = filtro.IdMateria

            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task AgregarPersonaAsync(AgregarPersonaExpedienteVm vm)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_DetallePersonaExpediente_Agregar",
            new { idExpediente = vm.IdExpediente, idPersona = vm.IdPersona, rolParticipante = vm.RolParticipante },
            commandType: CommandType.StoredProcedure);
    }

    public async Task AgregarMateriaAsync(AgregarMateriaExpedienteVm vm)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_DetalleMateriaExpediente_Agregar",
            new { idExpediente = vm.IdExpediente, idMateria = vm.IdMateria, descripcionDtMateria = vm.DescripcionDtMateria },
            commandType: CommandType.StoredProcedure);
    }

    public async Task EliminarAsync(int idExpediente)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_Expediente_Eliminar",
            new { idExpediente },
            commandType: CommandType.StoredProcedure);
    }
}
