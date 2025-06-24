using AgriConnect.Models;
using AgriConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Security.Cryptography;

namespace AgriConnect.Controllers
{
    //this is only accessible to employees and this manages the farmer CRUD operations
    [Authorize(Roles = "Employee")] //(Rick-Anderson, 2024)
    public class FarmersController : Controller
    {
        private readonly ITableStorageService<FarmerEntity> _farmerService;
        private readonly ITableStorageService<ProductEntity> _productService;

        public FarmersController(ITableStorageService<FarmerEntity> farmerService, ITableStorageService<ProductEntity> productService)
        {
            _farmerService = farmerService;
            _productService = productService;
        }

        // this shows all of the farmers
        public async Task<IActionResult> Index()
        {
            try
            {
                var farmers = await _farmerService.GetAllEntitiesAsync();
                return View(farmers);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to load farmers: {ex.Message}";
                return View(new List<FarmerEntity>());
            }
        }

        //displays the create farmer form
        public IActionResult Create()
        {
            return View();
        }

        // this handles all of the farmer creation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FarmerEntity farmer)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fix validation errors and try again";
                return View(farmer);
            }

            // this sets partition and row keys
            farmer.PartitionKey = "Farmers";
            farmer.RowKey = farmer.Username;

            // this hashes the password
            farmer.PasswordHash = HashPassword(farmer.PasswordHash);

            try
            {
                // checks if the farmer already exists
                var existingFarmer = await _farmerService.GetEntityAsync("Farmers", farmer.Username);
                if (existingFarmer != null)
                {
                    ModelState.AddModelError("Username", "Username already exists");
                    return View(farmer);
                }

                await _farmerService.AddEntityAsync(farmer);
                TempData["SuccessMessage"] = "Farmer created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Failed to save farmer: {ex.Message}");
                return View(farmer);
            }
        }

        // this hashes the farmers password before storing it in azure
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create(); // (www.youtube.com, n.d.)
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashBytes);
        }

        //deletes the farmer and their products
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            try
            {
                var allProducts = await _productService.GetAllEntitiesAsync();

                var productsToDelete = allProducts
                    .Where(p => p.FarmerId.Equals(rowKey, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var product in productsToDelete)
                {
                    await _productService.DeleteEntityAsync(product.PartitionKey, product.RowKey);
                }

                await _farmerService.DeleteEntityAsync(partitionKey, rowKey);

                TempData["SuccessMessage"] = "Farmer and thier products have been deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting farmer or products: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
