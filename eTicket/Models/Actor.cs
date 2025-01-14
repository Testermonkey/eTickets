using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eTicket.Models
{
    public class Actor
    {
        [Key]
        public int id { get; set; }
        public string ProfilePictureURL { get; set; }
        public string FullName { get; set; }
        public string Bio { get; set; }

        //Relationship
        public List<Actor_Movie> Actors_Movies { get; set; }
    }
}
