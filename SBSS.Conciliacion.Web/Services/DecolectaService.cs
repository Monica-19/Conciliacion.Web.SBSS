using System.Net.Http.Headers;
using System.Net.Http.Json;
using SBSS.Conciliacion.Web.Models;

namespace SBSS.Conciliacion.Web.Services;

public class DecolectaService : IDecolectaService
{
    private readonly HttpClient _http;
    private readonly ILogger<DecolectaService> _logger;

    public DecolectaService(HttpClient http, IConfiguration config, ILogger<DecolectaService> logger)
    {
        _logger = logger;

        var baseUrl = config["Decolecta:BaseUrl"] ?? "https://api.decolecta.com/v1";
        var apiKey = config["Decolecta:ApiKey"];

        http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        http.Timeout = TimeSpan.FromSeconds(10);

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        _http = http;
    }

    public async Task<DecolectaDniResponse?> ConsultarDniAsync(string numeroDni, CancellationToken ct = default)
    {
        try
        {
            using var respuesta = await _http.GetAsync($"reniec/dni?numero={numeroDni}", ct);
            if (!respuesta.IsSuccessStatusCode)
            {
                _logger.LogWarning("Decolecta DNI {Numero} respondió {Codigo}", numeroDni, (int)respuesta.StatusCode);
                return null;
            }

            return await respuesta.Content.ReadFromJsonAsync<DecolectaDniResponse>(cancellationToken: ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "Error consultando DNI {Numero} en Decolecta", numeroDni);
            return null;
        }
    }

    public async Task<DecolectaRucResponse?> ConsultarRucAsync(string numeroRuc, CancellationToken ct = default)
    {
        try
        {
            using var respuesta = await _http.GetAsync($"sunat/ruc?numero={numeroRuc}", ct);
            if (!respuesta.IsSuccessStatusCode)
            {
                _logger.LogWarning("Decolecta RUC {Numero} respondió {Codigo}", numeroRuc, (int)respuesta.StatusCode);
                return null;
            }

            return await respuesta.Content.ReadFromJsonAsync<DecolectaRucResponse>(cancellationToken: ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "Error consultando RUC {Numero} en Decolecta", numeroRuc);
            return null;
        }
    }
}
