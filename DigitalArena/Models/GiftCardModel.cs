using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class GiftCardModel
    {
        public int GiftCardId { get; set; }
        public string GiftCode { get; set; }
        public decimal Amount { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public Nullable<DateTime> UsedAt { get; set; }
        public Nullable<int> UserId { get; set; }

        public virtual UserModel User { get; set; }
    }
}