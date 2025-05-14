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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Security.Cryptography;

namespace TestProject
{
    public class UnitTest3
    {
        [Fact]
        public async Task Login_ValidUserCredentials_ShouldRedirectToRedirectByRole()
        {
            // Arrange
            var mockUserService = new Mock<ITableStorageService<ApplicationUser>>();
            var mockFarmerService = new Mock<ITableStorageService<FarmerEntity>>();

            var loginModel = new LoginViewModel
            {
                UserName = "testuser",
                Password = "ValidPass123"
            };

            var hashedPassword = Convert.ToBase64String(
                SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("ValidPass123")));

            var existingUser = new ApplicationUser
            {
                PartitionKey = "Users",
                RowKey = "testuser",
                UserName = "testuser",
                Email = "test@example.com",
                PasswordHash = hashedPassword,
                Role = "Admin"
            };

            mockUserService.Setup(s => s.GetEntityAsync("Users", "testuser"))
                           .ReturnsAsync(existingUser);

            var controller = new AccountController(mockUserService.Object, mockFarmerService.Object)
            {
                // Optional: skip actual sign-in during test
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.Login(loginModel);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("RedirectByRole", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);
        }
    }
}
