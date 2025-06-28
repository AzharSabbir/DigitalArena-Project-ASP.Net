using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Thumbnail { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
        public int UnlikeCount { get; set; }
        public string Status { get; set; }
        public int CategoryId { get; set; }
        public int SellerId { get; set; }
        public int ViewCount { get; set; }
        public double TrendScore { get; set; } 

        public virtual CategoryModel Category { get; set; }
        public virtual UserModel User { get; set; }
    }
}