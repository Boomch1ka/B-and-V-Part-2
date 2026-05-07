using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace B_and_V_Part_2.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        public string EventName { get; set; }

        public DateTime EventDate { get; set; }

        public string Description { get; set; }

        public ICollection<Booking> Bookings { get; set; }
    }
}