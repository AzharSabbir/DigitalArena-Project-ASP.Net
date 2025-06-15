using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class CartModel
    {
        public int CartId { get; set; }
        public int UserId { get; set; }

        public virtual UserModel User { get; set; }
    }
}