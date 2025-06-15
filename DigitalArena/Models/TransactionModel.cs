using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class TransactionModel
    {
        public int TransactionId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public int WalletId { get; set; }
        public virtual WalletModel Wallet { get; set; }
    }
}