using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class CouponModel
    {
        public int CouponId { get; set; }
        public string CouponCode { get; set; }
        public int DiscountPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int MaxUsage { get; set; }
        public int UsageCount { get; set; }
        public bool UserSpecific { get; set; }
        public Nullable<int> UserId { get; set; }
        public int CreatedBy { get; set; }

        public virtual UserModel User { get; set; }
    }
}