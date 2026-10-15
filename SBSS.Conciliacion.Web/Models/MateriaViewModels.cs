using System.ComponentModel.DataAnnotations;

namespace SBSS.Conciliacion.Web.Models;

public class TipoMateria
{
    public int IdTipoMateria { get; set; }
    public string NombreTipoMateria { get; set; } = string.Empty;
    public string? DescripcionTipoMateria { get; set; }
}

public class Materia
{
    public int IdMateria { get; set; }
    public int IdTipoMateria { get; set; }
    public string NombreMateria { get; set; } = string.Empty;
    public string? DescripcionMateria { get; set; }
    public string? NombreTipoMateria { get; set; }
}

public class CrearTipoMateriaVm
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nombre del tipo de materia")]
    public string NombreTipoMateria { get; set; } = string.Empty;

    [Display(Name = "Descripción")]
    [DataType(DataType.MultilineText)]
    public string? DescripcionTipoMateria { get; set; }
}

public class CrearMateriaVm
{
    [Required(ErrorMessage = "Selecciona el tipo de materia.")]
    [Display(Name = "Tipo de materia")]
    public int IdTipoMateria { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Materia")]
    public string NombreMateria { get; set; } = string.Empty;

    [Display(Name = "Descripción")]
    [DataType(DataType.MultilineText)]
    public string? DescripcionMateria { get; set; }
}

public class EditarMateriaVm
{
    [Required]
    public int IdMateria { get; set; }

    [Required(ErrorMessage = "Selecciona el tipo de materia.")]
    [Display(Name = "Tipo de materia")]
    public int IdTipoMateria { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Materia")]
    public string NombreMateria { get; set; } = string.Empty;

    [Display(Name = "Descripción")]
    [DataType(DataType.MultilineText)]
    public string? DescripcionMateria { get; set; }
}
