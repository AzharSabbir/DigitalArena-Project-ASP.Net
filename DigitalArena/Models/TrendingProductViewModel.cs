using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class TrendingProductViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
        public int UnlikeCount { get; set; }
        public int ViewCount { get; set; }
        public int DownloadCount { get; set; }

        public decimal Price { get; set; }                
        public string CategoryName { get; set; }          

        public RatingInfo Ratings { get; set; }
        public double TrendScore { get; set; }

        public class RatingInfo
        {
            public double Average { get; set; }
            public int Count { get; set; }
        }
    }

}