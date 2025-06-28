using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models.Custom
{
    public class SellerDetailsViewModel
    {
        public UserModel Seller { get; set; }

        public int ProductCount { get; set; }

        public int TotalOrders { get; set; }

        public decimal TotalRevenue { get; set; }

        public double AverageRating { get; set; }

        public int TotalReviews { get; set; }
    }
}