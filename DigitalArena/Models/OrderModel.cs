using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class OrderModel
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string BillingAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public Nullable<int> TransactionId { get; set; }
        public int UserId { get; set; }

        public virtual TransactionModel Transaction { get; set; }
        public virtual UserModel User { get; set; }
    }
}