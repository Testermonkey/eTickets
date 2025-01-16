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
    public class ActorsService : IActorsService
    {
        private readonly AppDbContext _context;
        public ActorsService(AppDbContext context)
        {
            _context = context;
        }

        //public object Cinemas { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        // Add Actor
        public async Task AddAsync(Actor actor)
        {
            await _context.Actors.AddAsync(actor);
            await _context.SaveChangesAsync();
        }
        // Delete Actor
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
        // Get All Actors
        public async Task<IEnumerable<Actor>> GetAllAsync()
        {
            var result = await _context.Actors.ToListAsync();
            return result;
        }
        // Get Actor by Id
        public async Task<Actor> GetByIdAsync(int id)
        {
           var result =  await _context.Actors.FirstOrDefaultAsync(n => n.Id == id);
            return result;
        }
        // Update Actor
        public async Task<Actor> UpdateAsync(int id, Actor newActor)
        {
            _context.Update(newActor);
            await _context.SaveChangesAsync();
            return newActor;
        }
    }
}
