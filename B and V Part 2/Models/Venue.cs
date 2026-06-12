using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace B_and_V_Part_2.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

        [Required]
        public string VenueName { get; set; }

        public string Location { get; set; }

        public int Capacity { get; set; }

        public string ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        public ICollection<Booking> Bookings { get; set; }
    }
}