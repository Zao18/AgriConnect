using Xunit;
using Moq;
using AgriConnect.Controllers;
using AgriConnect.Models;
using AgriConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace TestProject
{
    public class UnitTest1
    {
        [Fact]
        public async Task CreateProduct_ShouldAddProduct_AndRedirectToIndex()
        {
            var mockProductService = new Mock<ITableStorageService<ProductEntity>>();
            var mockFarmerService = new Mock<ITableStorageService<FarmerEntity>>();

            var testProduct = new ProductEntity
            {
                Name = "Test Apple",
                Category = "Fruits",
                ProductionDate = DateTime.UtcNow,
                FarmerId = "farmer123"
            };

            mockProductService.Setup(s => s.AddEntityAsync(It.IsAny<ProductEntity>()))
                              .Returns(Task.CompletedTask);

            var controller = new ProductsController(mockProductService.Object, mockFarmerService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "farmer123"),
                new Claim(ClaimTypes.Role, "Farmer")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var tempData = new TempDataDictionary(controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            var result = await controller.Create(testProduct);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            mockProductService.Verify(s => s.AddEntityAsync(It.IsAny<ProductEntity>()), Times.Once);
        }
    }
}