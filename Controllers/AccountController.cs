using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_PROJECT.Extensions;
using MVC_PROJECT.Models;
using MVC_PROJECT.Services.Interfaces;
using MVC_PROJECT.ViewModels.Account;
using System.Security.Claims;

namespace MVC_PROJECT.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid request data." });
            }

            var user = await _accountService.ValidateLoginAsync(model.UserName, model.Password);

            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Invalid username or password." });
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            // Sign in with cookie authentication
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            var redirectUrl = _accountService.GetRedirectUrlByRole(user);

            return Ok(new
            {
                success = true,
                role = user.Role.ToString(),
                userName = user.UserName,
                fullName = user.FullName,
                redirectUrl = redirectUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok(new { success = true, message = "Logged out successfully." });
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return StatusCode(403, new { success = false, message = "Access denied." });
        }

        [HttpGet]
        [Authorize]
        public IActionResult Me()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized(new { success = false, message = "Not authenticated." });
            }

            return Ok(new
            {
                isAuthenticated = true,
                userId = User.GetUserId(),
                fullName = User.Identity.Name,
                role = User.GetUserRole()
            });
        }
    }
}
