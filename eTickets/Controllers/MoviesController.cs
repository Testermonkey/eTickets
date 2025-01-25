using eTickets.Data;
using eTickets.Data.Services;
using eTickets.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace eTickets.Controllers
{
    [Authorize]
    public class MoviesController : Controller
    {
        private readonly IMoviesService _service;

        // Constructor to inject the IMoviesService dependency
        public MoviesController(IMoviesService service)
        {
            _service = service;
        }

        //GET: Movies/Index - Get all movies
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Fetch all movies including their related Cinema data
            var allMovies = await _service.GetAllAsync(n => n.Cinema);
            return View(allMovies);
        }

        // GET: Movies/Filter - Filter movies based on a search string
        [AllowAnonymous]
        public async Task<IActionResult> Filter(string searchString)
        {
            // Fetch all movies including their related Cinema data
            var allMovies = await _service.GetAllAsync(n => n.Cinema);

            // Filter movies based on the search string
            if (!string.IsNullOrEmpty(searchString))
            {
                var filteredResult = allMovies.Where(n => n.Name.Contains(searchString) || n.Description.Contains(searchString)).ToList();
                return View("Index", filteredResult);
            }
            return View("Index",allMovies);


        }

        // GET: Movies/Details/1 - Get details of a specific movie by id
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {   // Fetch movie details by id
            var movieDetail = await _service.GetMovieByIdAsync(id);
            return View(movieDetail);
        }

        // GET: Movies/Create - Display the form to create a new movie
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            // Fetch dropdown values for Cinemas, Producers, and Actors
            var movieDropdownsData = await _service.GetNewMovieDropdownsValues();

            // Populate ViewBag with dropdown data
            ViewBag.Cinemas = new SelectList(movieDropdownsData.Cinemas, "Id","Name");
            ViewBag.Producers = new SelectList(movieDropdownsData.Producers, "Id", "FullName");
            ViewBag.Actors = new SelectList(movieDropdownsData.Actors, "Id", "FullName");
            return View();
        }

        // POST: Movies/Create - Handle the form submission to create a new movie
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(NewMovieVM movie)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                // Fetch dropdown values again if the model state is invalid
                var movieDropdownsData = await _service.GetNewMovieDropdownsValues();

                // Populate ViewBag with dropdown data
                ViewBag.Cinemas = new SelectList(movieDropdownsData.Cinemas, "Id", "Name");
                ViewBag.Producers = new SelectList(movieDropdownsData.Producers, "Id", "FullName");
                ViewBag.Actors = new SelectList(movieDropdownsData.Actors, "Id", "FullName");
                return View(movie);
            }

            // Add the new movie to the database
            await _service.AddNewMovieAsync(movie);
            return RedirectToAction(nameof(Index));

        }

        // GET: Movies/Edit/1 - Display the form to edit an existing movie
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            // Fetch movie details by id
            var movieDetails = await _service.GetMovieByIdAsync(id);
            if (movieDetails == null) return View("NotFound");

            // Map movie details to the view model
            var response = new NewMovieVM()
            {
                Id = movieDetails.Id,
                Name = movieDetails.Name,
                Description = movieDetails.Description,
                Price = movieDetails.Price,
                StartDate = movieDetails.StartDate,
                EndDate = movieDetails.EndDate,
                ImageURL = movieDetails.ImageURL,
                MovieCategory = movieDetails.MovieCategory,
                CinemaId = movieDetails.CinemaId,
                ProducerId = movieDetails.ProducerId,
                ActorIds = movieDetails.Actors_Movies.Select(n => n.ActorId).ToList(),
            };

            // Fetch dropdown values for Cinemas, Producers, and Actors
            var movieDropdownsData = await _service.GetNewMovieDropdownsValues();
            ViewBag.Cinemas = new SelectList(movieDropdownsData.Cinemas, "Id", "Name");
            ViewBag.Producers = new SelectList(movieDropdownsData.Producers, "Id", "FullName");
            ViewBag.Actors = new SelectList(movieDropdownsData.Actors, "Id", "FullName");
            return View(response);
        }

        // POST: Movies/Edit/1 - Handle the form submission to edit an existing movie
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, NewMovieVM movie)
        {
            // Check if the movie id matches the id in the route
            if (id != movie.Id)
            {
                return View("NotFound");
            }

            // Validate the model state
            if (!ModelState.IsValid)
            {
                // Fetch dropdown values again if the model state is invalid
                var movieDropdownsData = await _service.GetNewMovieDropdownsValues();

                // Populate ViewBag with dropdown data
                ViewBag.Cinemas = new SelectList(movieDropdownsData.Cinemas, "Id", "Name");
                ViewBag.Producers = new SelectList(movieDropdownsData.Producers, "Id", "FullName");
                ViewBag.Actors = new SelectList(movieDropdownsData.Actors, "Id", "FullName");
                return View(movie);
            }

            // Update the movie in the database
            await _service.UpdateMovieAsync(movie);
            return RedirectToAction(nameof(Index));

        }

    }
}
