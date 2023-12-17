using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.Models;
using WebApplication1.Utilities;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public AccountController(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel user)
        {
            if (ModelState.IsValid)
            {
                var authenticatedUser = AuthenticateUser(user.Email, user.Password);

                if (authenticatedUser != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, authenticatedUser.FirstName + " " + authenticatedUser.LastName),
                        new Claim(ClaimTypes.Email, authenticatedUser.Email),
                        new Claim(ClaimTypes.Role, authenticatedUser.Role),
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        // Set properties for authentication (e.g., isPersistent)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    if (authenticatedUser.Role == "Admin")
                    {
                        return RedirectToAction("Index", "Products");
                    }

                    return RedirectToAction("Index", "Dashboard");
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password");
            }

            return View(user);
        }

        private User? AuthenticateUser(string email, string password)
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Email == email);

            if (user != null && PasswordHasher.VerifyPassword(user.Password, password))
            {
                return user;
            }

            return null;
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(User userModel)
        {
            if (ModelState.IsValid)
            {
                if (!dbContext.Users.Any(u => u.Email == userModel.Email))
                {
                    userModel.Password = PasswordHasher.HashPassword(userModel.Password);

                    dbContext.Users.Add(userModel);
                    dbContext.SaveChanges();

                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "Email already exists.");
                }
            }

            return View(userModel);
        }
    }
}
