using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public partial class LandingPageModel
    {
        public int LandingPageId { get; set; }
        public int ProductId { get; set; }
        public string Headline { get; set; }
        public string SubHeadline { get; set; }
        public string HeroImage { get; set; }

        public virtual Product Product { get; set; }
    }
}