using Microsoft.AspNetCore.Mvc;
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

                HttpContext.Session.SetInt32("IdUsuario", responseData.IdUsuario);
                HttpContext.Session.SetString("NombreUsuario", responseData.Nombre);
                HttpContext.Session.SetInt32("PuntosUsuario", responseData.Kilometros);

                return Ok(new { success = true, redirectUrl = "/Home/Index" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error conectando a la API: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Error de conexión con la API de Python." });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { success = true, redirectUrl = "/Login" });
        }
    }
}