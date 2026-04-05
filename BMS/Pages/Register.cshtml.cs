using BMS.Data;
using BMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BMS.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _passwordHasher;

        public RegisterModel(ApplicationDbContext db, IPasswordHasher<User> passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required, StringLength(100)]
            public string Username { get; set; } = string.Empty;

            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
            public string Password { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (await _db.Users.AnyAsync(u => u.Username == Input.Username))
            {
                ModelState.AddModelError(string.Empty, "Username already taken.");
                return Page();
            }

            if (await _db.Users.AnyAsync(u => u.Email == Input.Email))
            {
                ModelState.AddModelError(string.Empty, "Email already registered.");
                return Page();
            }

            var user = new User
            {
                Username = Input.Username,
                Email = Input.Email
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, Input.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToPage("/Dashboard");
        }
    }
}
