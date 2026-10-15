using System.Data;

namespace SBSS.Conciliacion.Web.Data;

/// <summary>
/// Crea conexiones nuevas hacia SQL Server. Cada repositorio abre y cierra
/// su propia conexión por operación (patrón recomendado con Dapper).
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
