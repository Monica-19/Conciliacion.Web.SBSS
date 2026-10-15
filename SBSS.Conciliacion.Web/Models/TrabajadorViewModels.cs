using System.ComponentModel.DataAnnotations;

namespace SBSS.Conciliacion.Web.Models;

public class Trabajador
{
    public int IdTrabajador { get; set; }
    public int? IdSedeRef { get; set; }
    public string DniTrabajador { get; set; } = string.Empty;
    public string NombreTrabajador { get; set; } = string.Empty;
    public string ApePaterTrabajador { get; set; } = string.Empty;
    public string ApeMaterTrabajador { get; set; } = string.Empty;
    public string? EmailTrabajador { get; set; }
    public string? TelefonoTrabajador { get; set; }
    public string? EstadoTrabajador { get; set; }
    public string? RegistroCivilComercial { get; set; }
    public string? NombreSede { get; set; }

    public string NombreCompleto => $"{NombreTrabajador} {ApePaterTrabajador} {ApeMaterTrabajador}";
}

public class CrearTrabajadorVm
{
    [Required(ErrorMessage = "Selecciona la sede.")]
    [Display(Name = "Sede")]
    public int IdSedeRef { get; set; }

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [StringLength(15)]
    [Display(Name = "DNI")]
    public string DniTrabajador { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nombres")]
    public string NombreTrabajador { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Apellido paterno")]
    public string ApePaterTrabajador { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido materno es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Apellido materno")]
    public string ApeMaterTrabajador { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    [Display(Name = "Correo")]
    public string? EmailTrabajador { get; set; }

    [Display(Name = "Teléfono")]
    public string? TelefonoTrabajador { get; set; }

    [Display(Name = "Registro de conciliador (RENAC) / colegiatura")]
    public string? RegistroCivilComercial { get; set; }
}

public class EditarTrabajadorVm
{
    [Required]
    public int IdTrabajador { get; set; }

    [Required(ErrorMessage = "Selecciona la sede.")]
    [Display(Name = "Sede")]
    public int IdSedeRef { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nombres")]
    public string NombreTrabajador { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Apellido paterno")]
    public string ApePaterTrabajador { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido materno es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Apellido materno")]
    public string ApeMaterTrabajador { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    [Display(Name = "Correo")]
    public string? EmailTrabajador { get; set; }

    [Display(Name = "Teléfono")]
    public string? TelefonoTrabajador { get; set; }
}

public class CambiarEstadoTrabajadorVm
{
    [Required]
    public int IdTrabajador { get; set; }

    [Required]
    [Display(Name = "Estado")]
    public string EstadoTrabajador { get; set; } = string.Empty;
}

/// <summary>Formulario para dar de alta el acceso al sistema de un trabajador ya registrado.</summary>
public class CrearAccesoVm
{
    [Required]
    public int IdTrabajador { get; set; }

    public string? NombreTrabajador { get; set; }

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Usuario")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasenia { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona un rol.")]
    [Display(Name = "Rol")]
    public string Rol { get; set; } = "CONCILIADOR";
}
