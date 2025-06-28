using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class ProductUploadViewModel
    {
        public int SellerId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public HttpPostedFileBase Thumbnail { get; set; }

        public IEnumerable<HttpPostedFileBase> Files { get; set; }
    }

}