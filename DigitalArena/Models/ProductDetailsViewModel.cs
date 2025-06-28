using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class ProductDetailsViewModel
    {
        public ProductModel Product { get; set; }
        public List<ReviewModel> Reviews { get; set; }
        public List<ProductModel> SuggestedProducts { get; set; }

        public bool IsInCart { get; set; }
        public bool IsLiked { get; set; }
        public bool IsDisliked { get; set; }
        public string PublishedAgo { get; set; }
        public int DownloadCount { get; set; }
        public string ModelPath { get; set; }
    }
}