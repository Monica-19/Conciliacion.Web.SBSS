using System.ComponentModel.DataAnnotations;

namespace SBSS.Conciliacion.Web.Models;

public class Sede
{
    public int IdSede { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? EstadoSede { get; set; }
}

public class CrearSedeVm
{
    [Required(ErrorMessage = "El nombre de la sede es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nombre de la sede")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Descripción / dirección")]
    [DataType(DataType.MultilineText)]
    public string? Descripcion { get; set; }
}

public class EditarSedeVm
{
    [Required]
    public int IdSede { get; set; }

    [Required(ErrorMessage = "El nombre de la sede es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nombre de la sede")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Descripción / dirección")]
    [DataType(DataType.MultilineText)]
    public string? Descripcion { get; set; }
}

public class CambiarEstadoSedeVm
{
    [Required]
    public int IdSede { get; set; }

    [Required]
    [Display(Name = "Estado")]
    public string EstadoSede { get; set; } = string.Empty;
}
