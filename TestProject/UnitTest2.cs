using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AgriConnect.Controllers;
using AgriConnect.Models;
using AgriConnect.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TestProject
{
    public class UnitTest2
    {
        [Fact]
        public async Task Register_Farmer_ShouldAddFarmer_WhenModelIsValid()
        {
            // Arrange
            var mockFarmerService = new Mock<ITableStorageService<FarmerEntity>>();
            var mockProductService = new Mock<ITableStorageService<ProductEntity>>();

            var newFarmer = new FarmerEntity
            {
                Username = "newfarmer",
                PasswordHash = "Test123!",
                FullName = "Test Farmer",
                Email = "test@example.com",
                PhoneNumber = "1234567890",
                Location = "Test Location"
            };

            mockFarmerService.Setup(s => s.GetEntityAsync("Farmers", "newfarmer"))
                             .ReturnsAsync((FarmerEntity?)null);

            mockFarmerService.Setup(s => s.AddEntityAsync(It.IsAny<FarmerEntity>()))
                             .Returns(Task.CompletedTask);

            var controller = new FarmersController(mockFarmerService.Object, mockProductService.Object);

            ValidateModel(newFarmer, controller);

            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            var result = await controller.Create(newFarmer);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            mockFarmerService.Verify(s => s.AddEntityAsync(It.IsAny<FarmerEntity>()), Times.Once);
        }

        private void ValidateModel(object model, Controller controller)
        {
            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);

            foreach (var validationResult in results)
            {
                foreach (var memberName in validationResult.MemberNames)
                {
                    controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage);
                }
            }
        }
    }
}
