using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Services;

/// <summary>
/// Genera los documentos legales en .docx (citaciones, actas) usando Open XML SDK,
/// sin depender de tener Word instalado en el servidor.
/// </summary>
public interface IDocumentoWordService
{
    byte[] GenerarActaConciliacion(Expediente expediente, ActaConciliacionVm acta);
    byte[] GenerarCitacionAudiencia(Expediente expediente, ProgramarAudienciaVm audiencia);
}
