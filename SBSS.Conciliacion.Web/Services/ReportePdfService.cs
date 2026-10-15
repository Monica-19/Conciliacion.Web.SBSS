using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Services;

public class ReportePdfService : IReportePdfService
{
    public ReportePdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerarReporteExpedientes(IEnumerable<ExpedienteListItemVm> expedientes)
    {
        var lista = expedientes.ToList();

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(30);
                pagina.DefaultTextStyle(x => x.FontSize(10));

                pagina.Header().Text("Reporte de Expedientes - Centro de Conciliación Extrajudicial SBSS")
                    .FontSize(14).Bold().FontColor("#0C3855");

                pagina.Content().Table(tabla =>
                {
                    tabla.ColumnsDefinition(columnas =>
                    {
                        columnas.RelativeColumn(2);
                        columnas.RelativeColumn(2);
                        columnas.RelativeColumn(2);
                        columnas.RelativeColumn(2);
                        columnas.RelativeColumn(3);
                    });

                    tabla.Header(encabezado =>
                    {
                        foreach (var titulo in new[] { "N.° Expediente", "Estado", "Recepción", "Vencimiento", "Conciliador" })
                        {
                            encabezado.Cell().Background("#0C3855").Padding(5)
                                .Text(titulo).FontColor("#FFFFFF").Bold();
                        }
                    });

                    foreach (var e in lista)
                    {
                        tabla.Cell().Padding(4).Text(e.NumeroExpediente);
                        tabla.Cell().Padding(4).Text(e.EstadoProceso ?? "-");
                        tabla.Cell().Padding(4).Text(e.FechaRecepcion?.ToString("dd/MM/yyyy") ?? "-");
                        tabla.Cell().Padding(4).Text(e.FechaVencimiento?.ToString("dd/MM/yyyy") ?? "-");
                        tabla.Cell().Padding(4).Text(e.Conciliador ?? "Sin asignar");
                    }
                });

                pagina.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Generado el ").FontSize(8);
                    t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8).Bold();
                });
            });
        });

        return documento.GeneratePdf();
    }

    public byte[] GenerarReportePorRango(IEnumerable<ReporteExpedienteVm> expedientes)
    {
        var lista = expedientes.ToList();

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4.Landscape());
                pagina.Margin(24);
                pagina.DefaultTextStyle(x => x.FontSize(8));

                pagina.Header().Text("Reporte de Expedientes por Rango de Fechas - Centro de Conciliación Extrajudicial SBSS")
                    .FontSize(13).Bold().FontColor("#0C3855");

                pagina.Content().Table(tabla =>
                {
                    tabla.ColumnsDefinition(columnas =>
                    {
                        columnas.RelativeColumn(2);
                        columnas.RelativeColumn(1.4f);
                        columnas.RelativeColumn(1.4f);
                        columnas.RelativeColumn(1.4f);
                        columnas.RelativeColumn(1.4f);
                        columnas.RelativeColumn(1.4f);
                        columnas.RelativeColumn(2.5f);
                        columnas.RelativeColumn(2);
                    });

                    tabla.Header(encabezado =>
                    {
                        foreach (var titulo in new[] { "N.° Expediente", "Estado", "Recepción", "Notificación", "Vencimiento", "Cierre", "Conciliador", "Sede" })
                        {
                            encabezado.Cell().Background("#0C3855").Padding(4)
                                .Text(titulo).FontColor("#FFFFFF").Bold();
                        }
                    });

                    foreach (var e in lista)
                    {
                        tabla.Cell().Padding(3).Text(e.NumeroExpediente);
                        tabla.Cell().Padding(3).Text(e.EstadoProceso ?? "-");
                        tabla.Cell().Padding(3).Text(e.FechaRecepcion?.ToString("dd/MM/yyyy") ?? "-");
                        tabla.Cell().Padding(3).Text(e.FechaNotificacion?.ToString("dd/MM/yyyy") ?? "-");
                        tabla.Cell().Padding(3).Text(e.FechaVencimiento?.ToString("dd/MM/yyyy") ?? "-");
                        tabla.Cell().Padding(3).Text(e.FechaCierre?.ToString("dd/MM/yyyy") ?? "-");
                        tabla.Cell().Padding(3).Text(e.Conciliador ?? "Sin asignar");
                        tabla.Cell().Padding(3).Text(e.Sede ?? "-");
                    }
                });

                pagina.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Generado el ").FontSize(8);
                    t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8).Bold();
                });
            });
        });

        return documento.GeneratePdf();
    }

    public byte[] GenerarReportePorMateria(IEnumerable<ReporteMateriaVm> materias)
    {
        var lista = materias.ToList();

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(30);
                pagina.DefaultTextStyle(x => x.FontSize(10));

                pagina.Header().Text("Reporte de Expedientes por Materia - Centro de Conciliación Extrajudicial SBSS")
                    .FontSize(14).Bold().FontColor("#0C3855");

                pagina.Content().Table(tabla =>
                {
                    tabla.ColumnsDefinition(columnas =>
                    {
                        columnas.RelativeColumn(2);
                        columnas.RelativeColumn(3);
                        columnas.RelativeColumn(1);
                    });

                    tabla.Header(encabezado =>
                    {
                        foreach (var titulo in new[] { "Tipo de materia", "Materia", "Cantidad" })
                        {
                            encabezado.Cell().Background("#0C3855").Padding(5)
                                .Text(titulo).FontColor("#FFFFFF").Bold();
                        }
                    });

                    foreach (var m in lista)
                    {
                        tabla.Cell().Padding(4).Text(m.NombreTipoMateria);
                        tabla.Cell().Padding(4).Text(m.NombreMateria);
                        tabla.Cell().Padding(4).Text(m.Cantidad.ToString());
                    }
                });

                pagina.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Generado el ").FontSize(8);
                    t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8).Bold();
                });
            });
        });

        return documento.GeneratePdf();
    }
}
