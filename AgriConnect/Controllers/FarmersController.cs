using AgriConnect.Models;
using AgriConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Security.Cryptography;

namespace AgriConnect.Controllers
{
    [Authorize(Roles = "Employee")]
    public class FarmersController : Controller
    {
        private readonly ITableStorageService<FarmerEntity> _farmerService;
        private readonly ITableStorageService<ProductEntity> _productService;

        public FarmersController(ITableStorageService<FarmerEntity> farmerService, ITableStorageService<ProductEntity> productService)
        {
            _farmerService = farmerService;
            _productService = productService;
        }

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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FarmerEntity farmer)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fix validation errors and try again.";
                return View(farmer);
            }

            // Set Partition and Row keys
            farmer.PartitionKey = "Farmers";
            farmer.RowKey = farmer.Username;

            // Hash password securely
            farmer.PasswordHash = HashPassword(farmer.PasswordHash);

            try
            {
                // Check if farmer already exists
                var existingFarmer = await _farmerService.GetEntityAsync("Farmers", farmer.Username);
                if (existingFarmer != null)
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    return View(farmer);
                }

                await _farmerService.AddEntityAsync(farmer);
                TempData["SuccessMessage"] = "Farmer created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Failed to save farmer: {ex.Message}");
                return View(farmer);
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashBytes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            try
            {
                // Step 1: Get all products
                var allProducts = await _productService.GetAllEntitiesAsync();

                // Step 2: Filter by farmer
                var productsToDelete = allProducts
                    .Where(p => p.FarmerId.Equals(rowKey, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Step 3: Delete each product
                foreach (var product in productsToDelete)
                {
                    await _productService.DeleteEntityAsync(product.PartitionKey, product.RowKey);
                }

                // Step 4: Delete farmer
                await _farmerService.DeleteEntityAsync(partitionKey, rowKey);

                TempData["SuccessMessage"] = "Farmer and related products deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting farmer or products: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
