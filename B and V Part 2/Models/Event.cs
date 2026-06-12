using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace B_and_V_Part_2.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        [Display(Name = "Event Name")]
        public string EventName { get; set; }

        [Display(Name = "Date")]
        public DateTime EventDate { get; set; }

        public string Description { get; set; }

        [Display(Name = "Event Type")]
        public int? EventTypeId { get; set; }
        public EventType EventType { get; set; }

        public string ImageUrl { get; set; }

        public ICollection<Booking> Bookings { get; set; }
    }
}