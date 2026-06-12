using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace B_and_V_Part_2.Models
{
    public class EventFilterViewModel
    {
        // Filter inputs
        public string Search { get; set; }
        public int? EventTypeId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // Results
        public IEnumerable<Event> Events { get; set; }

        // Dropdown options
        public IEnumerable<SelectListItem> EventTypes { get; set; }
    }
}