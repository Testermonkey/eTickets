using eTicket.Data.Base;
using eTickets.Data;
using eTickets.Models;
using Microsoft.EntityFrameworkCore;
using System;

//using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eTicket.Data.Services
{
    public class ActorsService : EntityBaseRepository<Actor>, IActorsService
    {
         public ActorsService(AppDbContext context) : base(context) { }
    }
}
