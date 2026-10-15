using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBSS.Conciliacion.Web.Models;
using SBSS.Conciliacion.Web.Repositories;
using SBSS.Conciliacion.Web.Services;

namespace SBSS.Conciliacion.Web.Controllers;

/// <summary>
/// Endpoints internos (llamados por fetch desde las vistas) que envuelven a
/// IDecolectaService. Validan el formato del documento ANTES de llamar a la
/// API externa, para no gastar cuota con números que ya sabemos inválidos.
/// </summary>
[Authorize]
[Route("api/consultas")]
public class ConsultasController : Controller
{
    private readonly IDecolectaService _decolecta;
    private readonly IUbigeoRepository _ubigeo;

    public ConsultasController(IDecolectaService decolecta, IUbigeoRepository ubigeo)
    {
        _decolecta = decolecta;
        _ubigeo = ubigeo;
    }

    [HttpGet("dni/{numero}")]
    public async Task<IActionResult> Dni(string numero, CancellationToken ct)
    {
        if (!Regex.IsMatch(numero, @"^\d{8}$"))
            return Ok(new ConsultaDocumentoResultado { Encontrado = false, Error = "El DNI debe tener 8 dígitos." });

        var resultado = await _decolecta.ConsultarDniAsync(numero, ct);
        if (resultado is null)
        {
            return Ok(new ConsultaDocumentoResultado
            {
                Encontrado = false,
                Error = "No se encontró el DNI o el servicio no está disponible en este momento."
            });
        }

        return Ok(new ConsultaDocumentoResultado
        {
            Encontrado = true,
            Nombres = resultado.Nombres,
            ApellidoPaterno = resultado.ApellidoPaterno,
            ApellidoMaterno = resultado.ApellidoMaterno
        });
    }

    [HttpGet("ruc/{numero}")]
    public async Task<IActionResult> Ruc(string numero, CancellationToken ct)
    {
        if (!Regex.IsMatch(numero, @"^\d{11}$"))
            return Ok(new ConsultaDocumentoResultado { Encontrado = false, Error = "El RUC debe tener 11 dígitos." });

        var resultado = await _decolecta.ConsultarRucAsync(numero, ct);
        if (resultado is null)
        {
            return Ok(new ConsultaDocumentoResultado
            {
                Encontrado = false,
                Error = "No se encontró el RUC o el servicio no está disponible en este momento."
            });
        }

        var salida = new ConsultaDocumentoResultado
        {
            Encontrado = true,
            RazonSocial = resultado.RazonSocial,
            Direccion = resultado.Direccion,
            ViaTipo = resultado.ViaTipo,
            ViaNombre = resultado.ViaNombre,
            Numero = resultado.Numero,
            Distrito = resultado.Distrito,
            Provincia = resultado.Provincia,
            Departamento = resultado.Departamento,
            Estado = resultado.Estado,
            Condicion = resultado.Condicion
        };

        // Intento best-effort de emparejar el distrito de SUNAT contra nuestro
        // catálogo ya sembrado, para autoseleccionar la cascada en el formulario.
        // Si no hay coincidencia (nombre distinto, catálogo incompleto, etc.),
        // simplemente no se autoselecciona nada — no es un error.
        if (!string.IsNullOrWhiteSpace(resultado.Distrito)
            && !string.IsNullOrWhiteSpace(resultado.Provincia)
            && !string.IsNullOrWhiteSpace(resultado.Departamento))
        {
            var match = await _ubigeo.BuscarDistritoPorNombresAsync(resultado.Distrito, resultado.Provincia, resultado.Departamento);
            if (match.HasValue)
            {
                salida.IdDepartamentoSugerido = match.Value.IdDepartamento;
                salida.IdProvinciaSugerido = match.Value.IdProvincia;
                salida.IdDistritoSugerido = match.Value.IdDistrito;
            }
        }

        return Ok(salida);
    }
}
