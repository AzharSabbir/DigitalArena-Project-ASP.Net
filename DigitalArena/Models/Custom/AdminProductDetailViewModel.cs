using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models.Custom
{
    public class AdminProductDetailViewModel
    {
        public Product Product { get; set; }
        public Category Category { get; set; }
        public User Seller { get; set; }
        public List<ProductFile> ProductFiles { get; set; }
        public List<Review> Reviews { get; set; }
        public int PurchaseCount { get; set; }
    }

}