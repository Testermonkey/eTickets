using eTickets.Data.Cart;
using eTickets.Data.Services;
using eTickets.Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace eTickets.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IMoviesService _moviesService;
        private readonly ShoppingCart _shoppingCart;
        private readonly IOrdersService _ordersService;

        // Constructor to inject dependencies
        public OrdersController(IMoviesService moviesService, ShoppingCart shoppingCart, IOrdersService ordersService)
        {
            _moviesService = moviesService;
            _shoppingCart = shoppingCart;
            _ordersService = ordersService;

        }

        // GET: Orders/Index - Get all orders for the current user
        public async Task<IActionResult> Index()
        {
            // Get the current user's ID and role
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string userRole = User.FindFirstValue(ClaimTypes.Role);

            // Fetch orders based on user ID and role
            var orders = await _ordersService.GetOrdersByUserIdAndRoleAsync(userId, userRole);
            return View(orders);
        }

        // GET: Orders/ShoppingCart - Display the shopping cart
        public IActionResult ShoppingCart()
        {
            // Get items in the shopping cart
            var items = _shoppingCart.GetShoppingCartItems();
            _shoppingCart.ShoppingCartItems = items;

            // Create a view model for the shopping cart
            var response = new ShoppingCartVM()
            {
                ShoppingCart = _shoppingCart,
                ShoppingCartTotal = _shoppingCart.GetShoppingCartTotal()
            };
            return View(response);
        }

        // POST: Orders/AddItemToShoppingCart - Add an item to the shopping cart
        public async Task<IActionResult> AddItemToShoppingCart(int id)
        {
            // Fetch the movie by ID
            var item = await _moviesService.GetMovieByIdAsync(id);

            // Add the item to the shopping cart if it exists
            if (item != null)
            {
                _shoppingCart.AddItemToCart(item);
            }
            return RedirectToAction(nameof(ShoppingCart));
        }

        // POST: Orders/RemoveItemFromShoppingCart - Remove an item from the shopping cart
        public async Task<IActionResult> RemoveItemFromShoppingCart(int id)
        {
            // Fetch the movie by ID
            var item = await _moviesService.GetMovieByIdAsync(id);

            // Remove the item from the shopping cart if it exists
            if (item != null)
            {
                _shoppingCart.RemoveItemFromCart(item);
            }
            return RedirectToAction(nameof(ShoppingCart));
        }

        // POST: Orders/CompleteOrder - Complete the order
        public async Task<IActionResult> CompleteOrder()
        {
            // Get items in the shopping cart
            var items = _shoppingCart.GetShoppingCartItems();
            // Get the current user's ID and email address
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string userEmailAddress = User.FindFirstValue(ClaimTypes.Email);

            // Store the order and clear the shopping cart
            await _ordersService.StoreOrderAsync(items, userId, userEmailAddress);
            await _shoppingCart.ClearShoppingCartAsync();

            return View("OrderCompleted");
        }
    }
}
