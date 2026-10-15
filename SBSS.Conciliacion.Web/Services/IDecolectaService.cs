using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Services;

/// <summary>
/// Cliente de la API de Decolecta (https://decolecta.gitbook.io/docs) para
/// autocompletar datos de personas naturales (RENIEC/DNI) y jurídicas
/// (SUNAT/RUC) a partir de solo el número de documento.
/// </summary>
public interface IDecolectaService
{
    Task<DecolectaDniResponse?> ConsultarDniAsync(string numeroDni, CancellationToken ct = default);
    Task<DecolectaRucResponse?> ConsultarRucAsync(string numeroRuc, CancellationToken ct = default);
}
