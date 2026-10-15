using System.Data;
using Dapper;
using SBSS.Conciliacion.Web.Data;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public class PersonaRepository : IPersonaRepository
{
    private readonly IDbConnectionFactory _factory;

    public PersonaRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<int> InsertarNaturalAsync(CrearPersonaNaturalVm vm)
    {
        using var conn = _factory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@idSede", vm.IdSede);
        parametros.Add("@celularP", vm.CelularP);
        parametros.Add("@correoP", vm.CorreoP);
        parametros.Add("@whatsappP", vm.WhatsappP);
        parametros.Add("@dniPersonaN", vm.DniPersonaN);
        parametros.Add("@nombrePersonaN", vm.NombrePersonaN);
        parametros.Add("@apellidoPaternoPersonaN", vm.ApellidoPaternoPersonaN);
        parametros.Add("@apellidoMaternoPersonaN", vm.ApellidoMaternoPersonaN);
        parametros.Add("@idPersonaNuevo", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync("sp_PersonaNatural_Insertar", parametros, commandType: CommandType.StoredProcedure);
        return parametros.Get<int>("@idPersonaNuevo");
    }

    public async Task<int> InsertarJuridicaAsync(CrearPersonaJuridicaVm vm)
    {
        using var conn = _factory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@idSede", vm.IdSede);
        parametros.Add("@celularP", vm.CelularP);
        parametros.Add("@correoP", vm.CorreoP);
        parametros.Add("@whatsappP", vm.WhatsappP);
        parametros.Add("@ruc", vm.Ruc);
        parametros.Add("@razonSocial", vm.RazonSocial);
        parametros.Add("@idPersonaNuevo", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync("sp_PersonaJuridica_Insertar", parametros, commandType: CommandType.StoredProcedure);
        return parametros.Get<int>("@idPersonaNuevo");
    }

    public async Task<PersonaDetalle?> ObtenerPorIdAsync(int idPersona)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<PersonaDetalle>(
            "sp_Persona_ObtenerPorId",
            new { idPersona },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<PersonaListItem>> ListarAsync(string? busqueda = null)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<PersonaListItem>(
            "sp_Persona_Listar",
            new { busqueda },
            commandType: CommandType.StoredProcedure);
    }

    public async Task CambiarEstadoAsync(int idPersona, string estadoPersona)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_Persona_CambiarEstado",
            new { idPersona, estadoPersona },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> InsertarDireccionAsync(DireccionVm direccion)
    {
        using var conn = _factory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@idDistrito", direccion.IdDistrito);
        parametros.Add("@tipoVia", direccion.TipoVia);
        parametros.Add("@nombreVia", direccion.NombreVia);
        parametros.Add("@numero", direccion.Numero);
        parametros.Add("@manzana", (string?)null);
        parametros.Add("@lote", (string?)null);
        parametros.Add("@urbanizacion", direccion.Urbanizacion);
        parametros.Add("@referencia", direccion.Referencia);
        parametros.Add("@codigoPostal", (string?)null);
        parametros.Add("@idDireccionNueva", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync("sp_Direccion_Insertar", parametros, commandType: CommandType.StoredProcedure);
        return parametros.Get<int>("@idDireccionNueva");
    }

    public async Task VincularDireccionAsync(int idPersona, int idDireccion, bool esPrincipal = true)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_DetalleDireccion_Insertar",
            new { idPersona, idDireccion, esPrincipal },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<DireccionDetalleVm?> ObtenerDireccionPrincipalAsync(int idPersona)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<DireccionDetalleVm>(
            "sp_Persona_ObtenerDireccionPrincipal",
            new { idPersona },
            commandType: CommandType.StoredProcedure);
    }
}
