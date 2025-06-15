using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class WalletModel
    {
        public int WalletId { get; set; }
        public decimal Balance { get; set; }
        public string Pin { get; set; }
        public int UserId { get; set; }
        public virtual UserModel User { get; set; }
    }
}