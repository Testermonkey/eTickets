using eTickets.Data;
using eTickets.Data.Services;
using eTickets.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eTickets.Controllers
{
    [Authorize]
    public class CinemasController : Controller
    {
        private readonly ICinemasService _service;

        // Constructor to inject the ICinemasService dependency
        public CinemasController(ICinemasService service)
        {
            _service = service;
        }

        // GET: Cinemas/Index - Get all cinemas
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Fetch all cinemas from the database
            var allCinemas = await _service.GetAllAsync();
            return View(allCinemas);
        }

        // GET: Cinemas/Create - Display the form to create a new cinema
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cinemas/Create - Handle the form submission to create a new cinema
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Logo,Name,Description")]Cinema cinema)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                return View(cinema);
            }

            // Add the new cinema to the database
            await _service.AddAsync(cinema);
            return RedirectToAction(nameof(Index));
        }

        // GET: Cinemas/Details/1 - Get details of a specific cinema by id
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            // Fetch cinema details by id
            var cinemaDetails = await _service.GetByIdAsync(id);
            if (cinemaDetails == null) return View("NotFound");
            return View(cinemaDetails);
        }

        // GET: Cinemas/Edit/1 - Display the form to edit an existing cinema
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            // Fetch cinema details by id
            var cinemaDetails = await _service.GetByIdAsync(id);
            if (cinemaDetails == null) return View("NotFound");
            return View(cinemaDetails);
        }

        // POST: Cinemas/Edit/1 - Handle the form submission to edit an existing cinema
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Logo,Name,Description")] Cinema cinema)
        {
            // Check if the cinema id matches the id in the route
            if (id != cinema.Id)
            {
                return View("NotFound");
            }

            // Validate the model state
            if (!ModelState.IsValid)
            {
                return View(cinema);
            }

            // Update the cinema in the database
            await _service.UpdateAsync(id, cinema);
            return RedirectToAction(nameof(Index));
        }

        // GET: Cinemas/Delete/1 - Display the confirmation page to delete a cinema
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            // Fetch cinema details by id
            var cinemaDetails = await _service.GetByIdAsync(id);
            if (cinemaDetails == null) return View("NotFound");
            return View(cinemaDetails);
        }

        // POST: Cinemas/Delete/1 - Handle the form submission to delete a cinema
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirm(int id)
        {   // Delete the cinema from the database
            var cinemaDetails = await _service.GetByIdAsync(id);
            if (cinemaDetails == null) return View("NotFound");

            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
 