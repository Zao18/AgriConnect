using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriConnect.Controllers
{
    //Only accessible to employees and Displays all users
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AdminDashboardController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        //Shows a list of users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        //Display the form to manually add a Farmer
        public IActionResult AddFarmer()
        {
            return View();
        }
    }
}
