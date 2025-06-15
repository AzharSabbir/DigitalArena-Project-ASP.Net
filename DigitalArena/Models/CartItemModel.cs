using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class CartItemModel
    {
        public int CartItemId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }

        public virtual CartModel Cart { get; set; }
        public virtual ProductModel Product { get; set; }
    }
}