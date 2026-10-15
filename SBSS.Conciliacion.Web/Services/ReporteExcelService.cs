using ClosedXML.Excel;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Services;

public class ReporteExcelService : IReporteExcelService
{
    public byte[] GenerarReporteExpedientes(IEnumerable<ExpedienteListItemVm> expedientes)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Expedientes");

        hoja.Cell(1, 1).Value = "N.° Expediente";
        hoja.Cell(1, 2).Value = "Estado";
        hoja.Cell(1, 3).Value = "Fecha de recepción";
        hoja.Cell(1, 4).Value = "Fecha de vencimiento";
        hoja.Cell(1, 5).Value = "Conciliador";
        hoja.Range(1, 1, 1, 5).Style.Font.Bold = true;
        hoja.Range(1, 1, 1, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#0C3855");
        hoja.Range(1, 1, 1, 5).Style.Font.FontColor = XLColor.White;

        var fila = 2;
        foreach (var e in expedientes)
        {
            hoja.Cell(fila, 1).Value = e.NumeroExpediente;
            hoja.Cell(fila, 2).Value = e.EstadoProceso;
            hoja.Cell(fila, 3).Value = e.FechaRecepcion;
            hoja.Cell(fila, 4).Value = e.FechaVencimiento;
            hoja.Cell(fila, 5).Value = e.Conciliador;
            fila++;
        }

        hoja.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerarReportePorRango(IEnumerable<ReporteExpedienteVm> expedientes)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Expedientes por rango");

        string[] encabezados = { "N.° Expediente", "Estado", "Recepción", "Notificación", "Vencimiento", "Cierre", "Conciliador", "Sede" };
        for (var i = 0; i < encabezados.Length; i++) hoja.Cell(1, i + 1).Value = encabezados[i];
        hoja.Range(1, 1, 1, encabezados.Length).Style.Font.Bold = true;
        hoja.Range(1, 1, 1, encabezados.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#0C3855");
        hoja.Range(1, 1, 1, encabezados.Length).Style.Font.FontColor = XLColor.White;

        var fila = 2;
        foreach (var e in expedientes)
        {
            hoja.Cell(fila, 1).Value = e.NumeroExpediente;
            hoja.Cell(fila, 2).Value = e.EstadoProceso;
            hoja.Cell(fila, 3).Value = e.FechaRecepcion;
            hoja.Cell(fila, 4).Value = e.FechaNotificacion;
            hoja.Cell(fila, 5).Value = e.FechaVencimiento;
            hoja.Cell(fila, 6).Value = e.FechaCierre;
            hoja.Cell(fila, 7).Value = e.Conciliador;
            hoja.Cell(fila, 8).Value = e.Sede;
            fila++;
        }

        hoja.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerarReportePorMateria(IEnumerable<ReporteMateriaVm> materias)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Por materia");

        hoja.Cell(1, 1).Value = "Tipo de materia";
        hoja.Cell(1, 2).Value = "Materia";
        hoja.Cell(1, 3).Value = "Cantidad de expedientes";
        hoja.Range(1, 1, 1, 3).Style.Font.Bold = true;
        hoja.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#0C3855");
        hoja.Range(1, 1, 1, 3).Style.Font.FontColor = XLColor.White;

        var fila = 2;
        foreach (var m in materias)
        {
            hoja.Cell(fila, 1).Value = m.NombreTipoMateria;
            hoja.Cell(fila, 2).Value = m.NombreMateria;
            hoja.Cell(fila, 3).Value = m.Cantidad;
            fila++;
        }

        hoja.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
