using eTickets.Data.Base;
using eTickets.Data;
using eTickets.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eTickets.Data.Services
{
    public class ActorsService : EntityBaseRepository<Actor>, IActorsService
    {
         public ActorsService(AppDbContext context) : base(context) { }
    }
}
