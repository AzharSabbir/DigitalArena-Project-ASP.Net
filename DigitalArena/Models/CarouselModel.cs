using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class CarouselModel
    {
        public int CarouselId { get; set; }
        public string Image { get; set; }
        public string Page { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}