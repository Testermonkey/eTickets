using eTickets.Data;
using eTickets.Data.Static;
using eTickets.Data.ViewModels;
using eTickets.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace eTickets.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;

        // Constructor to inject dependencies
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: Account/Users - Get all users
        public async Task<IActionResult> Users()
        {
            // Fetch all users from the database
            var users = await _context.Users.ToListAsync();

            return View(users);
        }

        // GET: Account/Login - Display the login form
        public IActionResult Login() => View(new LoginVM());

        // POST: Account/Login - Handle the login form submission
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }

            // Find the user by email
            var user = await _userManager.FindByEmailAsync(loginVM.EmailAddress);
            if (user != null)
            {
                // Check the user's password
                var passwordCheck = await _userManager.CheckPasswordAsync(user, loginVM.Password);
                if (passwordCheck)
                {
                    // Sign in the user
                    var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Movies");
                    }
                }
                TempData["Error"] = "Wrong Credentials. Please try again";
                return View(loginVM);
            }
            TempData["Error"] = "Wrong Credentials. Please try again";
            return View(loginVM);
        }

        // GET: Account/Register - Display the registration form
        public IActionResult Register() => View(new RegisterVM());

        // POST: Account/Register - Handle the registration form submission
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            // Check if the email is already in use
            var user = await _userManager.FindByEmailAsync(registerVM.EmailAddress);
            if (user != null)
            {
                TempData["Error"] = "This email is already in use!";
                return View(registerVM);
            }

            // Create a new user
            var newUser = new ApplicationUser()
            {
                FullName = registerVM.FullName,
                Email = registerVM.EmailAddress,
                UserName = registerVM.EmailAddress
            };

            // Add the new user to the database
            var newUserResponse = await _userManager.CreateAsync(newUser, registerVM.Password);

            if (newUserResponse.Succeeded)
                await _userManager.AddToRoleAsync(newUser, UserRoles.User);
            return View("RegisterCompleted");
        }

        // POST: Account/Logout - Handle the logout action
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Movies");
        }

        // GET: Account/AccessDenied - Display the access denied page
        public IActionResult AccessDenied(string ReturnUrl)
        {
            return View();
        }
    }
}
