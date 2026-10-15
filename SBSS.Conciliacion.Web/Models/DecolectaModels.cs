using System.Text.Json.Serialization;

namespace SBSS.Conciliacion.Web.Models;

/// <summary>
/// Respuesta de GET /v1/reniec/dni (Decolecta). Los nombres de propiedad usan
/// JsonPropertyName porque la API responde en snake_case (first_name, etc.)
/// y el resto del proyecto usa PascalCase en C#.
/// </summary>
public class DecolectaDniResponse
{
    [JsonPropertyName("first_name")]
    public string? Nombres { get; set; }

    [JsonPropertyName("first_last_name")]
    public string? ApellidoPaterno { get; set; }

    [JsonPropertyName("second_last_name")]
    public string? ApellidoMaterno { get; set; }

    [JsonPropertyName("full_name")]
    public string? NombreCompleto { get; set; }

    [JsonPropertyName("document_number")]
    public string? NumeroDocumento { get; set; }
}

/// <summary>Respuesta de GET /v1/sunat/ruc (Decolecta, consulta básica).</summary>
public class DecolectaRucResponse
{
    [JsonPropertyName("razon_social")]
    public string? RazonSocial { get; set; }

    [JsonPropertyName("numero_documento")]
    public string? NumeroDocumento { get; set; }

    [JsonPropertyName("estado")]
    public string? Estado { get; set; }

    [JsonPropertyName("condicion")]
    public string? Condicion { get; set; }

    [JsonPropertyName("direccion")]
    public string? Direccion { get; set; }

    [JsonPropertyName("ubigeo")]
    public string? Ubigeo { get; set; }

    [JsonPropertyName("via_tipo")]
    public string? ViaTipo { get; set; }

    [JsonPropertyName("via_nombre")]
    public string? ViaNombre { get; set; }

    [JsonPropertyName("numero")]
    public string? Numero { get; set; }

    [JsonPropertyName("distrito")]
    public string? Distrito { get; set; }

    [JsonPropertyName("provincia")]
    public string? Provincia { get; set; }

    [JsonPropertyName("departamento")]
    public string? Departamento { get; set; }
}

/// <summary>Resultado uniforme que devuelven los endpoints internos /api/consultas/* al frontend.</summary>
public class ConsultaDocumentoResultado
{
    public bool Encontrado { get; set; }
    public string? Error { get; set; }

    // DNI
    public string? Nombres { get; set; }
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }

    // RUC — datos de la empresa
    public string? RazonSocial { get; set; }
    public string? Estado { get; set; }
    public string? Condicion { get; set; }

    // RUC — dirección, para prellenar el bloque de ubigeo del formulario
    public string? Direccion { get; set; }
    public string? ViaTipo { get; set; }
    public string? ViaNombre { get; set; }
    public string? Numero { get; set; }
    public string? Distrito { get; set; }
    public string? Provincia { get; set; }
    public string? Departamento { get; set; }

    // Si el sistema logró emparejar el distrito de SUNAT contra el catálogo
    // de ubigeo ya sembrado, aquí van los IDs para autoseleccionar la cascada.
    public int? IdDepartamentoSugerido { get; set; }
    public int? IdProvinciaSugerido { get; set; }
    public int? IdDistritoSugerido { get; set; }
}
