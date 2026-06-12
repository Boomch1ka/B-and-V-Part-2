using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace B_and_V_Part_2.Models
{
    public class EventType
    {
        public int EventTypeId { get; set; }

        [Required]
        [Display(Name = "Event Type")]
        public string Name { get; set; }
    }
}