using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Frontend.Models.Dtos;
using Frontend.Services;

namespace Frontend.Controllers
{
    public class AuthController : Controller
    {
        private readonly LibraryApiClient _apiClient;

        public AuthController(LibraryApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var response = await _apiClient.LoginAsync(request);
            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                // Store token in cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(7)
                };
                Response.Cookies.Append("AuthToken", response.Token, cookieOptions);
                
                await SignInUserAsync(response, request.Email);

                TempData["Success"] = "Welcome back!";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(request);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var response = await _apiClient.RegisterAsync(request);
            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(7)
                };
                Response.Cookies.Append("AuthToken", response.Token, cookieOptions);

                await SignInUserAsync(response, request.Email);

                TempData["Success"] = "Account created successfully!";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Registration failed. Email might already be in use.");
            return View(request);
        }

        private async Task SignInUserAsync(AuthResponse response, string email)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, response.FullName),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, string.IsNullOrEmpty(response.Role) ? "Member" : response.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Delete("AuthToken");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Logged out successfully.";
            return RedirectToAction("Login");
        }
    }
}
