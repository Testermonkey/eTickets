using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace eTickets.Data.Base
{
    // Base repository interface for entities with common CRUD operations.
    public interface IEntityBaseRepository<T> where T : class, IEntityBase, new()
    {
        // Gets all entities.
        Task<IEnumerable<T>> GetAllAsync();

        // Gets all entities with included properties.
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includeProperties);

        // Gets an entity by its unique identifier.
        Task<T> GetByIdAsync(int id);

        // Gets an entity by its unique identifier with included properties.
        Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includeProperties);

        // Adds a new entity.
        Task AddAsync(T entity);

        // Updates an existing entity.
        Task UpdateAsync(int id, T entity);

        // Deletes an entity by its unique identifier.
        Task DeleteAsync(int id);
    }
}