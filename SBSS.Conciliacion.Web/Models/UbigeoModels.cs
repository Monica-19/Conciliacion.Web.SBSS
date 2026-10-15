using System.ComponentModel.DataAnnotations;

namespace SBSS.Conciliacion.Web.Models;

public class Departamento
{
    public int IdDepartamento { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class Provincia
{
    public int IdProvincia { get; set; }
    public int IdDepartamento { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class Distrito
{
    public int IdDistrito { get; set; }
    public int IdProvincia { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>Bloque de dirección, embebido (opcional) en los formularios de Persona.</summary>
public class DireccionVm
{
    [Display(Name = "Departamento")]
    public int? IdDepartamento { get; set; }

    [Display(Name = "Provincia")]
    public int? IdProvincia { get; set; }

    [Display(Name = "Distrito")]
    public int? IdDistrito { get; set; }

    [Display(Name = "Tipo de vía")]
    public string? TipoVia { get; set; }

    [Display(Name = "Nombre de vía")]
    public string? NombreVia { get; set; }

    [Display(Name = "Número")]
    public string? Numero { get; set; }

    [Display(Name = "Urbanización")]
    public string? Urbanizacion { get; set; }

    [Display(Name = "Referencia")]
    public string? Referencia { get; set; }

    /// <summary>True si hay al menos un dato de dirección para guardar (todo es opcional).</summary>
    public bool TieneDatos =>
        IdDistrito.HasValue || !string.IsNullOrWhiteSpace(NombreVia) || !string.IsNullOrWhiteSpace(Urbanizacion);
}

/// <summary>Resultado de sp_Persona_ObtenerDireccionPrincipal, para mostrar en Personas/Details.</summary>
public class DireccionDetalleVm
{
    public int IdDireccion { get; set; }
    public string? TipoVia { get; set; }
    public string? NombreVia { get; set; }
    public string? Numero { get; set; }
    public string? Manzana { get; set; }
    public string? Lote { get; set; }
    public string? Urbanizacion { get; set; }
    public string? Referencia { get; set; }
    public string? CodigoPostal { get; set; }
    public string Distrito { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
}
