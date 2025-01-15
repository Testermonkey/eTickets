using eTickets.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace eTicket.Controllers
{
    public class ActorsController : Controller
    {
        private readonly AppDbContext _context;

        public ActorsController(AppDbContext context)
        {
            _context = context;
        }

        // synchronous method
        public IActionResult Index()
        {
            var data = _context.Actors.ToList();
            return View(data);
        }
    }
}
