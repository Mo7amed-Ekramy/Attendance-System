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

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _accountService.ValidateLoginAsync(model.UserName, model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // Claims
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Role, user.Role.ToString())
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            var redirectUrl = _accountService.GetRedirectUrlByRole(user);

            return Redirect(redirectUrl);
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
