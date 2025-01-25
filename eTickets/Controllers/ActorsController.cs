using eTickets.Data;
using eTickets.Data.Services;
using eTickets.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eTickets.Controllers
{
    [Authorize]
    public class ActorsController : Controller
    {
        private readonly IActorsService _service;

        // Constructor to inject the IActorsService dependency
        public ActorsController(IActorsService service)
        {
            _service = service; 
        }

        // GET: Actors/Index - Get all actors
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Fetch all actors from the database
            var data = await _service.GetAllAsync();
            return View(data);
        }

        // GET: Actors/Create - Display the form to create a new actor
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create - Handle the form submission to create a new actor
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([Bind("FullName,ProfilePictureURL,Bio")]Actor actor)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                return View(actor);
            }

            // Add the new actor to the database
            await _service.AddAsync(actor);
            return RedirectToAction(nameof(Index));
        }

        // GET: Actors/Details/1 - Get details of a specific actor by id
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            // Fetch actor details by id
            var actorDetails = await _service.GetByIdAsync(id);
            if (actorDetails == null)  return View("NotFound"); 
            return View(actorDetails);
        }

        // GET: Actors/Edit/1 - Display the form to edit an existing actor
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            // Fetch actor details by id
            var actorDetails = await _service.GetByIdAsync(id);
            if (actorDetails == null) return View("NotFound");
            return View(actorDetails);
        }

        // POST: Actors/Edit/1 - Handle the form submission to edit an existing actor
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,ProfilePictureURL,Bio")] Actor actor)
        {
            // Validate the model state
            if (id != actor.Id)
            { 
                return View("NotFound"); 
            }
            if (!ModelState.IsValid)
            {
                return View(actor);
            }
            // Update the actor in the database
            await _service.UpdateAsync(id, actor);
            return RedirectToAction(nameof(Index));
        }

        // GET: Actors/Delete/1 - Display the confirmation page to delete an actor
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            // Fetch actor details by id
            var actorDetails = await _service.GetByIdAsync(id);
            if (actorDetails == null) return View("NotFound");
            return View(actorDetails);
        }

        // POST: Actors/Delete/1 - Handle the form submission to delete an actor
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Delete the actor from the database
            var actorDetails = await _service.GetByIdAsync(id);
            if (actorDetails == null) return View("NotFound");

            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
