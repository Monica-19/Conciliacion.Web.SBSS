using System.ComponentModel.DataAnnotations;

namespace SBSS.Conciliacion.Web.Models;

public class PersonaListItem
{
    public int IdPersona { get; set; }
    public string? CorreoP { get; set; }
    public string? CelularP { get; set; }
    public string? EstadoPersona { get; set; }
    public string TipoPersona { get; set; } = string.Empty; // NATURAL | JURIDICA
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Documento { get; set; }
}

public class PersonaDetalle
{
    public int IdPersona { get; set; }
    public string? CelularP { get; set; }
    public string? CorreoP { get; set; }
    public string? WhatsappP { get; set; }
    public string? EstadoPersona { get; set; }
    public string TipoPersona { get; set; } = string.Empty;

    public string? DniPersonaN { get; set; }
    public string? NombrePersonaN { get; set; }
    public string? ApellidoPaternoPersonaN { get; set; }
    public string? ApellidoMaternoPersonaN { get; set; }

    public string? Ruc { get; set; }
    public string? RazonSocial { get; set; }

    public string NombreCompleto => TipoPersona == "NATURAL"
        ? $"{NombrePersonaN} {ApellidoPaternoPersonaN} {ApellidoMaternoPersonaN}"
        : RazonSocial ?? string.Empty;

    public string Documento => TipoPersona == "NATURAL" ? DniPersonaN ?? string.Empty : Ruc ?? string.Empty;
}

public class CrearPersonaNaturalVm
{
    public int? IdSede { get; set; }

    [Display(Name = "Celular")]
    public string? CelularP { get; set; }

    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    [Display(Name = "Correo")]
    public string? CorreoP { get; set; }

    [Display(Name = "WhatsApp")]
    public string? WhatsappP { get; set; }

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [StringLength(15)]
    [Display(Name = "DNI")]
    public string DniPersonaN { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nombres")]
    public string NombrePersonaN { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Apellido paterno")]
    public string ApellidoPaternoPersonaN { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido materno es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Apellido materno")]
    public string ApellidoMaternoPersonaN { get; set; } = string.Empty;

    /// <summary>Dirección (opcional).</summary>
    public DireccionVm Direccion { get; set; } = new();
}

public class CrearPersonaJuridicaVm
{
    public int? IdSede { get; set; }

    [Display(Name = "Celular")]
    public string? CelularP { get; set; }

    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    [Display(Name = "Correo")]
    public string? CorreoP { get; set; }

    [Display(Name = "WhatsApp")]
    public string? WhatsappP { get; set; }

    [Required(ErrorMessage = "El RUC es obligatorio.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "El RUC debe tener 11 dígitos.")]
    [Display(Name = "RUC")]
    public string Ruc { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razón social es obligatoria.")]
    [StringLength(150)]
    [Display(Name = "Razón social")]
    public string RazonSocial { get; set; } = string.Empty;

    /// <summary>Dirección (opcional; se puede prellenar desde la consulta SUNAT).</summary>
    public DireccionVm Direccion { get; set; } = new();
}

public class CambiarEstadoPersonaVm
{
    [Required]
    public int IdPersona { get; set; }

    [Required]
    [Display(Name = "Estado")]
    public string EstadoPersona { get; set; } = string.Empty;
}

/// <summary>Vincular una persona ya registrada a un expediente (rol: reclamante, invitado, etc.).</summary>
public class AgregarPersonaExpedienteVm
{
    [Required]
    public int IdExpediente { get; set; }

    [Required(ErrorMessage = "Selecciona una persona.")]
    [Display(Name = "Persona")]
    public int IdPersona { get; set; }

    [Required(ErrorMessage = "Indica el rol que cumple en el expediente.")]
    [Display(Name = "Rol")]
    public string RolParticipante { get; set; } = string.Empty;
}

/// <summary>Vincular una materia a un expediente.</summary>
public class AgregarMateriaExpedienteVm
{
    [Required]
    public int IdExpediente { get; set; }

    [Required(ErrorMessage = "Selecciona una materia.")]
    [Display(Name = "Materia")]
    public int IdMateria { get; set; }

    [Display(Name = "Detalle")]
    [DataType(DataType.MultilineText)]
    public string? DescripcionDtMateria { get; set; }
}

/// <summary>Valores sugeridos para el rol de una persona dentro de un expediente.</summary>
public static class RolesParticipante
{
    public const string Solicitante = "SOLICITANTE";
    public const string Invitado = "INVITADO";
    public const string Apoderado = "APODERADO";
    public const string Testigo = "TESTIGO";

    public static readonly Dictionary<string, string> Etiquetas = new()
    {
        [Solicitante] = "Solicitante",
        [Invitado] = "Invitado",
        [Apoderado] = "Apoderado",
        [Testigo] = "Testigo"
    };
}
