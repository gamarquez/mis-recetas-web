using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MisRecetas.Web.Data;
using MisRecetas.Web.Models;

namespace MisRecetas.Web.Services;

/// <summary>
/// Servicio de autenticación con migración automática de contraseñas a BCrypt.
/// </summary>
public class AuthService
{
    private readonly UsuarioRepository _usuarioRepo;
    private readonly IHttpContextAccessor _httpContext;

    public AuthService(UsuarioRepository usuarioRepo, IHttpContextAccessor httpContext)
    {
        _usuarioRepo = usuarioRepo;
        _httpContext = httpContext;
    }

    /// <summary>
    /// Valida credenciales con soporte de migración texto plano → BCrypt.
    /// Retorna el usuario autenticado o null si las credenciales son inválidas.
    /// </summary>
    public Usuario? ValidarCredenciales(string nombreUsuario, string password)
    {
        var usuario = _usuarioRepo.ObtenerPorNombreUsuario(nombreUsuario);
        if (usuario is null) return null;

        bool passwordValida;

        if (EsBCryptHash(usuario.Password))
        {
            // Contraseña ya hasheada → verificar directamente
            passwordValida = BCrypt.Net.BCrypt.Verify(password, usuario.Password);
        }
        else
        {
            // Contraseña en texto plano → comparar y migrar si coincide
            passwordValida = usuario.Password == password;
            if (passwordValida)
            {
                var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
                _usuarioRepo.ActualizarPassword(usuario.Id_Usuario, hash);
                usuario.Password = hash;
            }
        }

        return passwordValida ? usuario : null;
    }

    /// <summary>
    /// Crea la cookie de autenticación con todos los claims necesarios.
    /// </summary>
    public async Task SignInAsync(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id_Usuario.ToString()),
            new(ClaimTypes.Name, usuario.Nombre_Usuario),
            new(ClaimTypes.GivenName, usuario.NombreCompleto),
            new(ClaimTypes.Role, usuario.DescripcionRol),
            new("IdRol", usuario.Id_Rol.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await _httpContext.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties);
    }

    public async Task SignOutAsync()
        => await _httpContext.HttpContext!.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

    /// <summary>
    /// Cambia la contraseña del usuario autenticado verificando la contraseña actual.
    /// </summary>
    public bool CambiarPassword(int idUsuario, string passwordActual, string nuevaPassword)
    {
        var usuario = _usuarioRepo.ObtenerPorId(idUsuario);
        if (usuario is null) return false;

        bool actualValida = EsBCryptHash(usuario.Password)
            ? BCrypt.Net.BCrypt.Verify(passwordActual, usuario.Password)
            : usuario.Password == passwordActual;

        if (!actualValida) return false;

        var nuevoHash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword, workFactor: 12);
        _usuarioRepo.ActualizarPassword(idUsuario, nuevoHash);
        return true;
    }

    // ── Helper ─────────────────────────────────────────────────────────────
    private static bool EsBCryptHash(string password)
        => password.StartsWith("$2") && password.Length >= 60;
}
