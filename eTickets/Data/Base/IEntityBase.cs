using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eTickets.Data.Base
{
    // Base interface for entities with an Id property.
    public interface IEntityBase
    {

        // Gets or sets the unique identifier for the entity.
        int Id { get; set; }
    }
}
