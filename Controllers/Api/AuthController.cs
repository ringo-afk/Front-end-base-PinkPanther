using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace PinkPanther.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public bool RememberMe { get; set; }
        }

        public class PythonApiResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public int IdUsuario { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public int Kilometros { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { success = false, message = "Faltan credenciales." });
            }

            var client = _httpClientFactory.CreateClient("PythonApi");
            var pythonApiUrl = "https://127.0.0.1:8000/api/login";

            var payload = new { email = request.Email, password = request.Password };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(pythonApiUrl, jsonContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    return Unauthorized(new { success = false, message = "Correo o contraseña incorrectos." });
                }

                var responseData = await response.Content.ReadFromJsonAsync<PythonApiResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
            return Ok(new { success = true, redirectUrl = "/Home/Login" });
        }
    }
}