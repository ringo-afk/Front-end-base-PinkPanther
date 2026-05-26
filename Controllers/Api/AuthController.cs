using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PinkPanther.Services;

namespace PinkPanther.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public bool RememberMe { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { success = false, message = "Faltan credenciales." });
            }

            try
            {
                var responseData = await _authService.AuthenticateAsync(request.Email, request.Password);

                if (responseData == null || !responseData.Success)
                {
                    return Unauthorized(new { success = false, message = responseData?.Message ?? "Correo o contraseña incorrectos." });
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, responseData.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, request.Email),
                    new Claim(ClaimTypes.GivenName, responseData.Nombre),
                    new Claim("Puntuacion", responseData.Kilometros.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties 
                    { 
                        IsPersistent = request.RememberMe,
                        ExpiresUtc = request.RememberMe ? DateTime.UtcNow.AddDays(7) : null
                    });

                return Ok(new { success = true, redirectUrl = "/Home/Index" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error conectando a la API: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Error de conexión con la API de Python." });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { success = true, redirectUrl = "/Login" });
        }
    }
}