using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using AgriConnect.Controllers;
using AgriConnect.Models;
using AgriConnect.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace UnitTestAccount
{
    [TestClass]
    public class AccountTest
    {
        private Mock<ITableStorageService<ApplicationUser>> _mockUserStorage;
        private Mock<ITableStorageService<FarmerEntity>> _mockFarmerStorage;
        private AccountController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockUserStorage = new Mock<ITableStorageService<ApplicationUser>>();
            _mockFarmerStorage = new Mock<ITableStorageService<FarmerEntity>>();
            _controller = new AccountController(_mockUserStorage.Object, _mockFarmerStorage.Object);

            // Mock SignIn
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(x => x.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), null))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddSingleton(authServiceMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task Register_ValidUser_RedirectsToLogin()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                Role = UserRole.Admin
            };

            _mockUserStorage.Setup(x => x.GetEntityAsync("Users", model.UserName)).ReturnsAsync((ApplicationUser)null);
            _mockUserStorage.Setup(x => x.AddEntityAsync(It.IsAny<ApplicationUser>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(model);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual("Login", redirect.ActionName);
        }

        [TestMethod]
        public async Task Login_ValidCredentials_RedirectsToRole()
        {
            // Arrange
            var model = new LoginViewModel
            {
                UserName = "admin",
                Password = "Admin123!"
            };

            var user = new ApplicationUser
            {
                PartitionKey = "Users",
                RowKey = "admin",
                UserName = "admin",
                Email = "admin@example.com",
                PasswordHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes("Admin123!"))),
                Role = "Admin"
            };

            _mockUserStorage.Setup(x => x.GetEntityAsync("Users", model.UserName)).ReturnsAsync(user);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual("RedirectByRole", redirect.ActionName);
        }

        [TestMethod]
        public async Task Login_InvalidCredentials_ShowsError()
        {
            // Arrange
            var model = new LoginViewModel
            {
                UserName = "wronguser",
                Password = "WrongPassword"
            };

            _mockUserStorage.Setup(x => x.GetEntityAsync("Users", model.UserName)).ReturnsAsync((ApplicationUser)null);
            _mockFarmerStorage.Setup(x => x.GetEntityAsync("Farmers", model.UserName)).ReturnsAsync((FarmerEntity)null);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsFalse(_controller.ModelState.IsValid);
        }
    }
}

