using eTickets.Data.Base;
using eTickets.Data;
using eTickets.Models;


//using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eTickets.Data.Services
{
    public class CinemasService : EntityBaseRepository<Cinema>, ICinemasService
    {
         public CinemasService(AppDbContext context) : base(context) { }
    }
}
