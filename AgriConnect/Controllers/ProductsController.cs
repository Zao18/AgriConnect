using AgriConnect.Models;
using AgriConnect.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AgriConnect.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ITableStorageService<FarmerEntity> _farmerService;
        private readonly ITableStorageService<ProductEntity> _productService;

        public ProductsController(ITableStorageService<ProductEntity> productService, ITableStorageService<FarmerEntity> farmerService)
        {
            _productService = productService;
            _farmerService = farmerService;
        }

        public async Task<IActionResult> Index(ProductFilterViewModel filter)
        {
            try
            {
                var userId = User.Identity.Name;
                var userRole = User.IsInRole("Farmer") ? "Farmer" : "Employee";

                var allProducts = await _productService.GetAllEntitiesAsync();

                if (userRole == "Farmer")
                {
                    allProducts = allProducts.Where(p => p.FarmerId == userId).ToList();
                }

                if (!string.IsNullOrEmpty(filter.ProductCategory))
                {
                    allProducts = allProducts
                        .Where(p => p.Category != null && p.Category.Contains(filter.ProductCategory, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrEmpty(filter.SelectedFarmerId) && userRole != "Farmer")
                {
                    allProducts = allProducts
                        .Where(p => p.FarmerId == filter.SelectedFarmerId)
                        .ToList();
                }

                if (filter.StartDate.HasValue)
                {
                    allProducts = allProducts
                        .Where(p => p.ProductionDate >= filter.StartDate.Value)
                        .ToList();
                }

                if (filter.EndDate.HasValue)
                {
                    allProducts = allProducts
                        .Where(p => p.ProductionDate <= filter.EndDate.Value)
                        .ToList();
                }

                var farmers = await _farmerService.GetAllEntitiesAsync();
                filter.FarmerOptions = farmers.Select(f => new SelectListItem
                {
                    Value = f.RowKey,
                    Text = $"{f.FullName} ({f.Username})"
                }).ToList();

                filter.FilteredProducts = allProducts;
                return View(filter);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while loading the products.";
                return View(new ProductFilterViewModel
                {
                    FilteredProducts = new List<ProductEntity>(),
                    FarmerOptions = new List<SelectListItem>()
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var farmers = await _farmerService.GetAllEntitiesAsync();
                ViewBag.Farmers = farmers
                    .Select(f => new SelectListItem
                    {
                        Value = f.RowKey,
                        Text = f.FullName + " (" + f.Username + ")"
                    }).ToList();

                return View();
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while preparing the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductEntity product)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new ApplicationException("Invalid form submission.");

                product.ProductionDate = DateTime.SpecifyKind(product.ProductionDate, DateTimeKind.Utc);

                if (User.IsInRole("Farmer"))
                    product.FarmerId = User.Identity.Name;

                product.PartitionKey = "Product";
                product.RowKey = Guid.NewGuid().ToString();

                await _productService.AddEntityAsync(product);

                TempData["Success"] = "Product created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while creating the product.";

                try
                {
                    var farmers = await _farmerService.GetAllEntitiesAsync();
                    ViewBag.Farmers = farmers.Select(f => new SelectListItem
                    {
                        Value = f.RowKey,
                        Text = f.FullName + " (" + f.Username + ")"
                    }).ToList();
                }
                catch
                {
                    ViewBag.Farmers = new List<SelectListItem>();
                }

                return View(product);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductEntity product)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(product);

                product.ProductionDate = DateTime.SpecifyKind(product.ProductionDate, DateTimeKind.Utc);
                await _productService.UpdateEntityAsync(product);

                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while updating the product.";
                return View(product);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            try
            {
                await _productService.DeleteEntityAsync(partitionKey, rowKey);
                TempData["Success"] = "Product deleted successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Failed to delete the product.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

