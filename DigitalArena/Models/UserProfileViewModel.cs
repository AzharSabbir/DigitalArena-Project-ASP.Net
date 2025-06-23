using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class UserProfileViewModel
    {
        public UserModel User { get; set; }
        public int TotalPurchases { get; set; }
        public bool IsSellerMode { get; set; }
        public List<ProductModel> PurchasedProducts { get; set; }
        public List<ProductModel> UploadedProducts { get; set; }

    }

}