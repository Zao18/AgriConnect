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
    //AccountController handles user registration login and logout and redirects based on user role
    public class AccountController : Controller
    {
        private readonly ITableStorageService<ApplicationUser> _tableStorage;
        private readonly ITableStorageService<FarmerEntity> _farmerService;

        public AccountController(ITableStorageService<ApplicationUser> tableStorage, ITableStorageService<FarmerEntity> farmerService)
        {
            _tableStorage = tableStorage;
            _farmerService = farmerService;
        }

        //display registration form
        [HttpGet]
        public IActionResult Register() => View();

        //handles the users registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Please correct the highlighted errors");
                return View(model);
            }

            try
            {
                //Checks if the user already exists
                var existingUser = await _tableStorage.GetEntityAsync("Users", model.UserName);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "A user already exists with that username");
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
                TempData["SuccessMessage"] = "Registration was successful";
                return RedirectToAction("Login", "Account");
            }
            catch (RequestFailedException ex)
            {
                ModelState.AddModelError("", "A storage error has occurred");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred");
            }

            return View(model);
        }

        //hashes the password using SHA-256
        private string HashPassword(string password) //(www.youtube.com, n.d.)
        {
            using var sha256 = SHA256.Create(); 
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashBytes);
        }

        //Checks if the password matches the stored hashed password
        private bool VerifyPassword(string storedHash, string password)
        {
            return storedHash == HashPassword(password);
        }

        //displays the login form
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // handles the login for the employee and the farmers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Please fill in all of the required fields");
                return View(model);
            }

            try
            {
                ApplicationUser user = null;
                FarmerEntity farmer = null;

                //this tries to find as employees or general user
                try
                {
                    user = await _tableStorage.GetEntityAsync("Users", model.UserName);
                }
                catch (RequestFailedException)
                {
                    // storage error or user not found
                }

                if (user != null && VerifyPassword(user.PasswordHash, model.Password))
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); //(Rick-Anderson, 2024b)
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    return RedirectToAction("RedirectByRole", "Account");
                }

                // tries as farmer
                try
                {
                    farmer = await _farmerService.GetEntityAsync("Farmers", model.UserName);
                }
                catch (RequestFailedException)
                {
                    //the farmer was not found
                }

                if (farmer != null && VerifyPassword(farmer.PasswordHash, model.Password))
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, farmer.FullName),
                new Claim(ClaimTypes.Email, farmer.Email),
                new Claim(ClaimTypes.Role, "Farmer")
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); // (Rick - Anderson, 2024b)
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    return RedirectToAction("RedirectByRole", "Account");
                }

                //employee or farmer was found or password was incorrect
                ModelState.AddModelError("", "Invalid username or password");
            }
            catch (RequestFailedException)
            {
                ModelState.AddModelError("", "A system error occurred");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred");
            }

            return View(model);
        }

        //Logs out the user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // (Rick - Anderson, 2024b)
            return RedirectToAction("Login", "Account");
        }

        //displays access denied page
        public IActionResult AccessDenied()
        {
            return View();
        }

        //Redirects logged in user based on thier role
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


