using BMS.Data;
using BMS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BMS.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _passwordHasher;

        public LoginModel(ApplicationDbContext db, IPasswordHasher<User> passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            public string UsernameOrEmail { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Page("/Dashboard");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Page("/Dashboard");

            if (!ModelState.IsValid) return Page();

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username == Input.UsernameOrEmail || u.Email == Input.UsernameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials.");
                return Page();
            }

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, Input.Password);
            if (verification == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials.");
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return LocalRedirect(ReturnUrl);
        }
    }
}
