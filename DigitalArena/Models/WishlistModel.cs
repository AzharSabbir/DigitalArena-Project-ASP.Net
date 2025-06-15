using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class WishlistModel
    {
        public int WishlistId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }

        public virtual ProductModel Product { get; set; }
        public virtual UserModel User { get; set; }
    }
}