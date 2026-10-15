using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Services;

public class DocumentoWordService : IDocumentoWordService
{
    public byte[] GenerarActaConciliacion(Expediente expediente, ActaConciliacionVm acta)
    {
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            body.AppendChild(Titulo("ACTA DE CONCILIACIÓN EXTRAJUDICIAL"));
            body.AppendChild(Subtitulo($"Expediente N.° {expediente.NumeroExpediente}"));
            body.AppendChild(Parrafo($"Ley N.° 26872 - Ley de Conciliación y su Reglamento, D.S. N.° 004-2005-JUS."));
            body.AppendChild(new Paragraph(new Run(new Break())));

            body.AppendChild(SeccionTitulo("I. DESCRIPCIÓN DE LA CONTROVERSIA"));
            body.AppendChild(Parrafo(acta.DescripcionControversia));

            body.AppendChild(SeccionTitulo("II. ACUERDO CONCILIATORIO"));
            body.AppendChild(Parrafo(acta.AcuerdoConciliatorio));

            body.AppendChild(new Paragraph(new Run(new Break())));
            body.AppendChild(Parrafo($"Trujillo, {DateTime.Now:dd 'de' MMMM 'de' yyyy}."));

            body.AppendChild(new SectionProperties());
        }

        return stream.ToArray();
    }

    public byte[] GenerarCitacionAudiencia(Expediente expediente, ProgramarAudienciaVm audiencia)
    {
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            body.AppendChild(Titulo("CITACIÓN A AUDIENCIA DE CONCILIACIÓN"));
            body.AppendChild(Subtitulo($"Expediente N.° {audiencia.NumeroExpediente}"));
            body.AppendChild(new Paragraph(new Run(new Break())));

            body.AppendChild(Parrafo(
                $"Se le cita a la audiencia de conciliación N.° {audiencia.NumeroSesion}, " +
                $"a realizarse el día {audiencia.Fecha:dd/MM/yyyy} a horas {audiencia.Hora:hh\\:mm}, " +
                "en las instalaciones del Centro de Conciliación Extrajudicial SBSS."));

            body.AppendChild(new Paragraph(new Run(new Break())));
            body.AppendChild(Parrafo("La inasistencia injustificada podrá generar los efectos previstos en la Ley N.° 26872 y su Reglamento."));

            body.AppendChild(new SectionProperties());
        }

        return stream.ToArray();
    }

    private static Paragraph Titulo(string texto) => new(
        new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
        new Run(
            new RunProperties(new Bold(), new FontSize { Val = "32" }),
            new Text(texto)));

    private static Paragraph Subtitulo(string texto) => new(
        new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
        new Run(
            new RunProperties(new FontSize { Val = "24" }, new Color { Val = "0C3855" }),
            new Text(texto)));

    private static Paragraph SeccionTitulo(string texto) => new(
        new Run(
            new RunProperties(new Bold(), new FontSize { Val = "24" }),
            new Text(texto)));

    private static Paragraph Parrafo(string texto) => new(
        new Run(new Text(texto) { Space = SpaceProcessingModeValues.Preserve }));
}
