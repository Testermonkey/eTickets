using eTickets.Data;
using eTickets.Data.Services;
using eTickets.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eTickets.Controllers
{
    [Authorize]
    public class ProducersController : Controller
    {
        private readonly IProducersService _service;

        // Constructor to inject the IProducersService dependency
        public ProducersController(IProducersService service)
        {
            _service = service;
        }

        // GET: Producers/Index - Get all producers
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Fetch all producers from the database
            var allProducers = await _service.GetAllAsync();
            return View(allProducers);
        }

        // GET: Producers/Details/1 - Get details of a specific producer by id
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            // Fetch producer details by id
            var producerDetails = await _service.GetByIdAsync(id);
            if (producerDetails == null) return View("NotFound");
            return View(producerDetails);
        }

        // GET: Producers/Create - Display the form to create a new producer
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Producers/Create - Handle the form submission to create a new producer
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([Bind("ProfilePictureURL,FullName,Bio")] Producer producer)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                return View(producer);
            }

            // Add the new producer to the database
            await _service.AddAsync(producer);
            return RedirectToAction(nameof(Index));
        }

        // GET: Producers/Edit/1 - Display the form to edit an existing producer
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            // Fetch producer details by id
            var producerDetails = await _service.GetByIdAsync(id);
            if (producerDetails == null) return View("NotFound");
            return View(producerDetails);
        }

        // POST: Producers/Edit/1 - Handle the form submission to edit an existing producer
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProfilePictureURL,FullName,Bio")] Producer producer)
        {
            // Check if the producer id matches the id in the route
            if (id != producer.Id)
            {
                return View("NotFound");
            }
            // Validate the model state
            if (!ModelState.IsValid)
            {
                return View(producer);
            }

            // Update the producer in the database
            await _service.UpdateAsync(id, producer);
            return RedirectToAction(nameof(Index));
        }

        // GET: Producers/Delete/1 - Display the confirmation page to delete a producer
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            // Fetch producer details by id
            var producerDetails = await _service.GetByIdAsync(id);
            if (producerDetails == null) return View("NotFound");
            return View(producerDetails);
        }

        // POST: Producers/Delete/1 - Handle the form submission to delete a producer
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Delete the producer from the database
            var producerDetails = await _service.GetByIdAsync(id);
            if (producerDetails == null) return View("NotFound");

            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
