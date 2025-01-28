using eTickets.Controllers;
using eTickets.Data;
using eTickets.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections;
using eTickets.Tests.TestHelpers;
using eTickets.Data.ViewModels;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using eTickets.Data.Static;

namespace eTickets.Tests
{
    [TestFixture]
    public class AccountControllerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private Mock<AppDbContext> _contextMock;
        private AccountController _controller;

        [SetUp]
        public void Setup()
        {
            // Purpose: 
            // The Setup method initializes the mocks for dependencies and prepares the 
            // AccountController for testing with a simulated environment.

            // Step 1: Set up the UserManager mock
            // The UserManager requires an IUserStore<ApplicationUser> dependency. 
            // Mocking it allows us to control the behavior of UserManager for testing purposes.
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Step 2: Set up the SignInManager mock
            // The SignInManager depends on the UserManager and additional components like IHttpContextAccessor
            // and IUserClaimsPrincipalFactory. Mocking these components ensures the SignInManager can be used
            // without requiring an actual HTTP context.
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object, contextAccessorMock.Object, userPrincipalFactoryMock.Object, null, null, null, null);

            // Step 3: Set up the AppDbContext mock
            // Mock the database context to simulate access to the Users DbSet.
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            _contextMock = new Mock<AppDbContext>(options);

            // Create a test list of ApplicationUser objects to simulate the database Users table.
            var users = new List<ApplicationUser>
    {
        new ApplicationUser { UserName = "user1" },
        new ApplicationUser { UserName = "user2" }
    }.AsQueryable();

            // Mock the DbSet to support IQueryable and asynchronous operations.
            var dbSetMock = new Mock<DbSet<ApplicationUser>>();
            dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
            dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
            dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            // Step 4: Mock asynchronous behavior for DbSet using IAsyncEnumerable
            dbSetMock.As<IAsyncEnumerable<ApplicationUser>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<ApplicationUser>(users.GetEnumerator()));

            dbSetMock.As<IQueryable<ApplicationUser>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<ApplicationUser>(users.Provider));

            // Setup the DbContext to return the mocked Users DbSet.
            _contextMock.Setup(c => c.Users).Returns(dbSetMock.Object);

            // Step 5: Initialize the AccountController
            // Inject the mocked dependencies into the controller for testing.
            _controller = new AccountController(_userManagerMock.Object, _signInManagerMock.Object, _contextMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Users_ReturnsViewResult_WithListOfUsers()
        {
            // Test Covers:
            //     - Retrieval of all users from the database.
            //     - Verifies that the Users action returns the correct view and model.
            // Expected Behavior:
            //     - The controller returns a ViewResult.
            //     - The ViewResult contains a model of type List<ApplicationUser>.
            //     - The model contains the correct users as expected in the database mock.

            // Act:
            // Call the Users action method to retrieve the list of users
            var result = await _controller.Users();

            // Assert:
            // Verify the result is a ViewResult
            Assert.That(result, Is.InstanceOf<ViewResult>(), "Expected a ViewResult");

            // Verify the model returned in the ViewResult is of type List<ApplicationUser>
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.InstanceOf<List<ApplicationUser>>(), "Expected the model to be of type List<ApplicationUser>");

            // Verify the model contains the expected users
            var model = viewResult?.Model as List<ApplicationUser>;
            Assert.That(model?.Count, Is.EqualTo(2), "Expected the model to contain exactly 2 users");
            Assert.That(model?[0].UserName, Is.EqualTo("user1"), "Expected the first user's username to be 'user1'");
            Assert.That(model?[1].UserName, Is.EqualTo("user2"), "Expected the second user's username to be 'user2'");
        }


        [Test]
        public void Login_ReturnsViewResult_WithLoginVM()
        {
            // Test Covers:
            //     - Initial GET request for the login page.
            //     - Verifies that the Login action (GET) returns the correct view and model.
            // Expected Behavior:
            //     - The controller returns a ViewResult.
            //     - The ViewResult contains a model of type LoginVM.

            // Act:
            // Call the Login action (GET) method
            var result = _controller.Login();

            // Assert:
            // Verify the result is a ViewResult
            Assert.That(result, Is.InstanceOf<ViewResult>(), "Expected a ViewResult");

            // Verify the model returned in the ViewResult is of type LoginVM
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.InstanceOf<LoginVM>(), "Expected the model to be of type LoginVM");
        }

        [Test]
        public async Task Login_WithInvalidModel_ReturnsViewResult()
        {
            // Test Covers:
            // Invalid Model State:
            //     - The LoginVM model provided to the Login action is invalid (e.g., missing required fields).
            // Expected Behavior:
            //     - The controller returns a ViewResult containing the same LoginVM model.
            //     - The ModelState remains invalid, so validation errors can be displayed in the view.

            // Arrange: Create an invalid LoginVM object and add a model state error
            var loginVM = new LoginVM { EmailAddress = "", Password = "" }; // Invalid model (empty fields)
            _controller.ModelState.AddModelError("EmailAddress", "Email is required.");

            // Act: Call the Login action with the invalid model
            var result = await _controller.Login(loginVM);

            // Assert: Verify the result is a ViewResult with the LoginVM model
            Assert.That(result, Is.InstanceOf<ViewResult>(), "Expected a ViewResult");
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.InstanceOf<LoginVM>(), "Expected the model to be of type LoginVM");
            Assert.That(viewResult?.Model, Is.EqualTo(loginVM), "Expected the original LoginVM to be returned");
        }

        [Test]
        public async Task Login_ValidCredentials_RedirectsToMoviesIndex()
        {
            // Test Covers:
            // Happy Path (Valid Credentials):
            //     - User exists in the database.
            //     - Password is correct.
            //     - Sign-in is successful.
            // Expected Behavior:
            //     - The controller redirects to the "Index" action in the "Movies" controller.

            // Arrange: Mock TempData and valid LoginVM
            var tempDataMock = new Mock<ITempDataDictionary>();
            _controller.TempData = tempDataMock.Object;

            var loginVM = new LoginVM { EmailAddress = "test@example.com", Password = "CorrectPassword" };
            var mockUser = new ApplicationUser { Email = "test@example.com", UserName = "testuser" };

            // Mock UserManager to find the user by email
            _userManagerMock.Setup(u => u.FindByEmailAsync(loginVM.EmailAddress))
                            .ReturnsAsync(mockUser);

            // Mock UserManager to check the password and confirm it is valid
            _userManagerMock.Setup(u => u.CheckPasswordAsync(mockUser, loginVM.Password))
                            .ReturnsAsync(true);

            // Mock SignInManager to simulate successful sign-in
            _signInManagerMock.Setup(s => s.PasswordSignInAsync(mockUser, loginVM.Password, false, false))
                              .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act: Call the Login action with valid credentials
            var result = await _controller.Login(loginVM);

            // Assert: Verify the result is a RedirectToActionResult to "Movies/Index"
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>(), "Expected a RedirectToActionResult");
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult?.ActionName, Is.EqualTo("Index"), "Expected action name to be 'Index'");
            Assert.That(redirectResult?.ControllerName, Is.EqualTo("Movies"), "Expected controller name to be 'Movies'");
        }

        [Test]
        public async Task Login_IncorrectPassword_ReturnsViewWithError()
        {
            // Test Covers:
            //     - Invalid login attempt:
            //         - The user exists in the database.
            //         - The provided password is incorrect (CheckPasswordAsync returns false).
            // Expected Behavior:
            //     - The controller returns a ViewResult containing the original LoginVM model.
            //     - TempData contains an error message indicating incorrect credentials.

            // Arrange:
            // Mock TempData for the controller to simulate storing error messages
            var tempDataMock = new Mock<ITempDataDictionary>();
            _controller.TempData = tempDataMock.Object;

            // Create a LoginVM object with an existing user's email and an incorrect password
            var loginVM = new LoginVM { EmailAddress = "test@example.com", Password = "WrongPassword" };

            // Mock an existing user returned by FindByEmailAsync
            var mockUser = new ApplicationUser { Email = "test@example.com", UserName = "testuser" };
            _userManagerMock.Setup(u => u.FindByEmailAsync(loginVM.EmailAddress))
                            .ReturnsAsync(mockUser);

            // Mock the password check to fail
            _userManagerMock.Setup(u => u.CheckPasswordAsync(mockUser, loginVM.Password))
                            .ReturnsAsync(false);

            // Act:
            // Call the Login method with the incorrect password
            var result = await _controller.Login(loginVM);

            // Assert:
            // Verify the result is a ViewResult
            Assert.That(result, Is.InstanceOf<ViewResult>(), "Expected a ViewResult");

            // Verify the model returned in the ViewResult is the original LoginVM
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.EqualTo(loginVM), "Expected the original LoginVM to be returned");

            // Verify TempData contains the appropriate error message
            tempDataMock.VerifySet(t => t["Error"] = "Wrong Credentials. Please try again", Times.Once);
        }

        [Test]
        public async Task Login_NonExistentUser_ReturnsViewWithError()
        {
            // Test Covers:
            //     - Invalid login attempt:
            //         - The user does not exist in the database (FindByEmailAsync returns null).
            // Expected Behavior:
            //     - The controller returns a ViewResult containing the original LoginVM model.
            //     - TempData contains an error message indicating incorrect credentials.

            // Arrange:
            // Mock TempData for the controller to simulate storing error messages
            var tempDataMock = new Mock<ITempDataDictionary>();
            _controller.TempData = tempDataMock.Object;

            // Create a LoginVM object with credentials for a non-existent user
            var loginVM = new LoginVM { EmailAddress = "nonexistent@example.com", Password = "Password123" };

            // Mock the UserManager to return null when searching for the user
            _userManagerMock.Setup(u => u.FindByEmailAsync(loginVM.EmailAddress))
                            .ReturnsAsync((ApplicationUser)null);

            // Act:
            // Call the Login method with the LoginVM object
            var result = await _controller.Login(loginVM);

            // Assert:
            // Verify the result is a ViewResult
            Assert.That(result, Is.InstanceOf<ViewResult>(), "Expected a ViewResult");

            // Verify the model returned in the ViewResult is the original LoginVM
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.EqualTo(loginVM), "Expected the original LoginVM to be returned");

            // Verify TempData contains the appropriate error message
            tempDataMock.VerifySet(t => t["Error"] = "Wrong Credentials. Please try again", Times.Once);
        }

        [Test]
        public async Task Login_SignInFails_ReturnsViewWithError()
        {
            // Test Covers:
            //     - Sign-in failure scenario:
            //         - User exists in the database.
            //         - Password is correct.
            //         - Sign-in fails due to external factors (e.g., user not allowed, account lockout, etc.).
            // Expected Behavior:
            //     - The controller returns a ViewResult containing the original LoginVM model.
            //     - TempData contains an error message indicating incorrect credentials.

            // Arrange:
            // Mock TempData for the controller to simulate storing error messages
            var tempDataMock = new Mock<ITempDataDictionary>();
            _controller.TempData = tempDataMock.Object;

            // Create a LoginVM object with valid credentials
            var loginVM = new LoginVM { EmailAddress = "test@example.com", Password = "CorrectPassword" };

            // Mock a user retrieved by email
            var mockUser = new ApplicationUser { Email = "test@example.com", UserName = "testuser" };
            _userManagerMock.Setup(u => u.FindByEmailAsync(loginVM.EmailAddress))
                            .ReturnsAsync(mockUser);

            // Mock successful password validation
            _userManagerMock.Setup(u => u.CheckPasswordAsync(mockUser, loginVM.Password))
                            .ReturnsAsync(true);

            // Mock a failed sign-in attempt
            _signInManagerMock.Setup(s => s.PasswordSignInAsync(mockUser, loginVM.Password, false, false))
                              .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act:
            // Call the Login method with the LoginVM object
            var result = await _controller.Login(loginVM);

            // Assert:
            // Verify the result is a ViewResult
            Assert.That(result, Is.InstanceOf<ViewResult>(), "Expected a ViewResult");

            // Verify the model returned in the ViewResult matches the original LoginVM
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.EqualTo(loginVM), "Expected the original LoginVM to be returned");

            // Verify TempData contains the appropriate error message
            tempDataMock.VerifySet(t => t["Error"] = "Wrong Credentials. Please try again", Times.Once);
        }

        [Test]
        public async Task Login_WithInvalidCredentials_ReturnsViewResult()
        {
            // Test Covers:
            //     - Invalid login attempt:
            //         - User does not exist in the database (FindByEmailAsync returns null).
            // Expected Behavior:
            //     - The controller returns a ViewResult containing the original LoginVM model.
            //     - TempData contains an error message indicating incorrect credentials.

            // Arrange:
            // Mock TempData for the controller to simulate storing error messages
            var tempDataMock = new Mock<ITempDataDictionary>();
            _controller.TempData = tempDataMock.Object;

            // Create a LoginVM object with invalid credentials
            var loginVM = new LoginVM { EmailAddress = "test@example.com", Password = "WrongPassword" };

            // Mock the UserManager to return null when searching for the user
            _userManagerMock.Setup(u => u.FindByEmailAsync(loginVM.EmailAddress))
                            .ReturnsAsync((ApplicationUser)null);

            // Act:
            // Call the Login method with the LoginVM object
            var result = await _controller.Login(loginVM);

            // Assert:
            // Verify the result is a ViewResult
            Assert.That(result, Is.InstanceOf<ViewResult>(), "Expected a ViewResult");

            // Verify the model returned in the ViewResult is the original LoginVM
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.InstanceOf<LoginVM>(), "Expected the model to be of type LoginVM");
            Assert.That(viewResult?.Model, Is.EqualTo(loginVM), "Expected the original LoginVM to be returned");

            // Verify TempData contains the appropriate error message
            tempDataMock.VerifySet(t => t["Error"] = "Wrong Credentials. Please try again", Times.Once);
        }

        [Test]
        public async Task Register_ValidModel_RedirectsToRegisterCompleted()
        {
            // Test Covers:
            // - Happy Path (Valid Registration):
            //     - The model state is valid.
            //     - The email is not already in use.
            //     - User creation is successful.
            //     - The user is assigned the "User" role.
            // - Expected Behavior:
            //     - The controller redirects to the "RegisterCompleted" view.

            // Arrange
            var registerVM = new RegisterVM
            {
                FullName = "Test User",
                EmailAddress = "test@example.com",
                Password = "Password123!"
            };

            // Mock UserManager to ensure the email is not already in use
            _userManagerMock.Setup(u => u.FindByEmailAsync(registerVM.EmailAddress))
                            .ReturnsAsync((ApplicationUser)null);

            // Mock UserManager to successfully create a user
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), registerVM.Password))
                            .ReturnsAsync(IdentityResult.Success);

            // Mock UserManager to successfully assign the "User" role
            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserRoles.User))
                            .Returns(Task.FromResult(IdentityResult.Success)); // Simulate successful role assignment

            // Act
            var result = await _controller.Register(registerVM);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>(), "Expected a ViewResult");
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.ViewName, Is.EqualTo("RegisterCompleted"), "Expected to redirect to the RegisterCompleted view");
        }



    }
}
