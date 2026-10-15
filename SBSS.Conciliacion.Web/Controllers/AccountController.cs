using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBSS.Conciliacion.Web.Models;
using SBSS.Conciliacion.Web.Repositories;

namespace SBSS.Conciliacion.Web.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IUsuarioRepository _usuarios;

    public AccountController(IUsuarioRepository usuarios) => _usuarios = usuarios;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity is { IsAuthenticated: true })
            return RedirectToAction("Index", "Dashboard");

        return View(new LoginVm { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var usuario = await _usuarios.ObtenerParaLoginAsync(vm.Usuario);

        var credencialesValidas = usuario is not null
            && BCrypt.Net.BCrypt.Verify(vm.Contrasenia, usuario.Contrasenia);

        if (!credencialesValidas)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View(vm);
        }

        if (!string.Equals(usuario!.EstadoTrabajador, "ACTIVO", StringComparison.OrdinalIgnoreCase)
            && usuario.EstadoTrabajador is not null)
        {
            ModelState.AddModelError(string.Empty, "Tu usuario no está activo. Contacta al administrador del sistema.");
            return View(vm);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Name, $"{usuario.NombreTrabajador} {usuario.ApePaterTrabajador}"),
            new("idTrabajador", usuario.IdTrabajador.ToString()),
            new(ClaimTypes.Role, usuario.Rol ?? "Trabajador")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // ---------------------------------------------------------------
    // Recuperación de contraseña.
    //
    // El sistema no tiene un servicio de correo configurado (no estaba
    // en el alcance original), así que en vez de "enviar un email" que
    // en realidad no llegaría a ningún lado, se muestra el enlace de
    // restablecimiento en pantalla para que el administrador se lo
    // haga llegar al usuario por el medio que corresponda. El único
    // cambio para conectarlo a un correo real sería reemplazar
    // MostrarEnlace() por una llamada a un IEmailSender.
    // ---------------------------------------------------------------

    [HttpGet]
    public IActionResult OlvideContrasenia() => View(new OlvideContraseniaVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OlvideContrasenia(OlvideContraseniaVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var usuario = await _usuarios.ObtenerParaLoginAsync(vm.Usuario);
        if (usuario is null)
        {
            // No revelamos si el usuario existe o no: mismo mensaje en ambos casos.
            TempData["Ok"] = "Si el usuario existe, se generó un enlace de restablecimiento.";
            return RedirectToAction(nameof(Login));
        }

        var token = GenerarTokenSeguro();
        await _usuarios.GenerarTokenResetAsync(usuario.IdUsuario, token, minutosValidez: 30);

        var enlace = Url.Action(nameof(RestablecerContrasenia), "Account", new { token }, Request.Scheme);
        ViewBag.Enlace = enlace;
        ViewBag.Usuario = usuario.Usuario;
        return View("EnlaceGenerado");
    }

    [HttpGet]
    public async Task<IActionResult> RestablecerContrasenia(string token)
    {
        var idUsuario = await _usuarios.ValidarTokenResetAsync(token);
        if (idUsuario is null)
        {
            TempData["Error"] = "El enlace de restablecimiento no es válido o ya expiró.";
            return RedirectToAction(nameof(Login));
        }

        return View(new RestablecerContraseniaVm { Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestablecerContrasenia(RestablecerContraseniaVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var idUsuario = await _usuarios.ValidarTokenResetAsync(vm.Token);
        if (idUsuario is null)
        {
            TempData["Error"] = "El enlace de restablecimiento no es válido o ya expiró.";
            return RedirectToAction(nameof(Login));
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(vm.NuevaContrasenia);
        await _usuarios.ActualizarContraseniaAsync(idUsuario.Value, hash);
        await _usuarios.MarcarTokenResetUsadoAsync(vm.Token);

        TempData["Ok"] = "Tu contraseña se actualizó correctamente. Ya puedes iniciar sesión.";
        return RedirectToAction(nameof(Login));
    }

    private static string GenerarTokenSeguro()
    {
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
