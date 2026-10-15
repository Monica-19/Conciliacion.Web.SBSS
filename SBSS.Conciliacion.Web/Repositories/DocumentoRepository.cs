using System.Data;
using Dapper;
using SBSS.Conciliacion.Web.Data;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Repositories;

public class DocumentoRepository : IDocumentoRepository
{
    private readonly IDbConnectionFactory _factory;

    public DocumentoRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<int> InsertarDocumentoGeneradoAsync(int idTipoDocumentos, int idExpediente)
    {
        using var conn = _factory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@idTipoDocumentos", idTipoDocumentos);
        parametros.Add("@idExpediente", idExpediente);
        parametros.Add("@idDocumentoNuevo", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync(
            "sp_DocumentoGenerado_Insertar",
            parametros,
            commandType: CommandType.StoredProcedure);

        return parametros.Get<int>("@idDocumentoNuevo");
    }

    public async Task RegistrarActaConciliacionAsync(ActaConciliacionVm vm)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_ActaConciliacion_Registrar",
            new
            {
                idDocumento = vm.IdDocumento,
                descripcionControversia = vm.DescripcionControversia,
                acuerdoConciliatorio = vm.AcuerdoConciliatorio
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task RegistrarActaNotificacionAsync(int idDocumento, DateTime fechaNotificacion, TimeSpan horaNotificacion)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_ActaNotificacion_Registrar",
            new { idDocumento, fechaNotificacion, horaNotificacion },
            commandType: CommandType.StoredProcedure);
    }

    public async Task InsertarAnexoAsync(int idDocumento, string nombreArchivo, string rutaArchivo)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_Anexo_Insertar",
            new { idDocumento, nombreArchivo, rutaArchivo },
            commandType: CommandType.StoredProcedure);
    }

    public async Task InsertarArchivoAdjuntoAsync(int idExpediente, int idTrabajador, string tipoArchivo, string nombreArchivo, string rutaArchivo, int? tamanioKB)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_ArchivoAdjunto_Insertar",
            new { idExpediente, idTrabajador, tipoArchivo, nombreArchivo, rutaArchivo, tamanioKB },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<ArchivoAdjuntoVm>> ListarArchivosPorExpedienteAsync(int idExpediente, string? tipoArchivo = null)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<ArchivoAdjuntoVm>(
            "sp_ArchivoAdjunto_ListarPorExpediente",
            new { idExpediente, tipoArchivo },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<ArchivoAdjuntoVm?> ObtenerArchivoPorIdAsync(int idArchivo)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ArchivoAdjuntoVm>(
            "sp_ArchivoAdjunto_ObtenerPorId",
            new { idArchivo },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<TipoDocumento>> ListarTiposDocumentoAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<TipoDocumento>(
            "sp_Documentos_Listar",
            commandType: CommandType.StoredProcedure);
    }
}
