using eTicket.Data.Services;
using eTickets.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace eTicket.Controllers
{
    public class CinemasController : Controller
    {
        private readonly IActorsService _service;

        public CinemasController(IActorsService service)
        {
            _service = service;
        }
        public async Task<IActionResult> Index()
        {
            var data = await _service.GetAll();
            return View(data);
        }
    }
}
