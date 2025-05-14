using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using AgriConnect.Models;
using AgriConnect.Services;


namespace AgriConnect.Controllers
{
    public class AccountController : Controller
    {
        private readonly ITableStorageService<ApplicationUser> _tableStorage;
        private readonly ITableStorageService<FarmerEntity> _farmerService;

        public AccountController(ITableStorageService<ApplicationUser> tableStorage, ITableStorageService<FarmerEntity> farmerService)
        {
            _tableStorage = tableStorage;
            _farmerService = farmerService;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Please correct the highlighted errors.");
                return View(model);
            }

            try
            {
                // Check if user already exists
                var existingUser = await _tableStorage.GetEntityAsync("Users", model.UserName);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "User already exists with this username.");
                    return View(model);
                }

                var userEntity = new ApplicationUser
                {
                    PartitionKey = "Users",
                    RowKey = model.UserName,
                    Email = model.Email,
                    UserName = model.UserName,
                    PasswordHash = HashPassword(model.Password),
                    Role = model.Role.ToString()
                };

                await _tableStorage.AddEntityAsync(userEntity);
                TempData["SuccessMessage"] = "Registration successful. Please log in.";
                return RedirectToAction("Login", "Account");
            }
            catch (RequestFailedException ex)
            {
                ModelState.AddModelError("", "A storage error occurred while creating the user. Please try again.");
                // Optionally log the exception: ex.Message
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                // Optionally log the exception: ex.Message
            }

            return View(model);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPassword(string storedHash, string password)
        {
            return storedHash == HashPassword(password);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Please fill in all required fields.");
                return View(model);
            }

            try
            {
                ApplicationUser user = null;
                FarmerEntity farmer = null;

                // Try to find as Admin or general user
                try
                {
                    user = await _tableStorage.GetEntityAsync("Users", model.UserName);
                }
                catch (RequestFailedException)
                {
                    // Storage error or user not found — continue to check farmers
                }

                if (user != null && VerifyPassword(user.PasswordHash, model.Password))
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    return RedirectToAction("RedirectByRole", "Account");
                }

                // Try as a Farmer
                try
                {
                    farmer = await _farmerService.GetEntityAsync("Farmers", model.UserName);
                }
                catch (RequestFailedException)
                {
                    // Farmer not found
                }

                if (farmer != null && VerifyPassword(farmer.PasswordHash, model.Password))
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, farmer.FullName),
                new Claim(ClaimTypes.Email, farmer.Email),
                new Claim(ClaimTypes.Role, "Farmer")
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    return RedirectToAction("RedirectByRole", "Account");
                }

                // Neither user nor farmer was found or password was incorrect
                ModelState.AddModelError("", "Invalid username or password.");
            }
            catch (RequestFailedException)
            {
                ModelState.AddModelError("", "A system error occurred while attempting to log in. Please try again.");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please contact support if the problem persists.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RedirectByRole()
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "AdminDashboard");

            if (User.IsInRole("Farmer"))
                return RedirectToAction("Index", "Products");

            return RedirectToAction("Index", "Home");
        }
    }
}


