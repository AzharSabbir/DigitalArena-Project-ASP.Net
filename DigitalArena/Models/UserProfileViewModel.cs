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
        public bool IsSellerMode => User.Role == "Seller";
        public List<ProductModel> PurchasedProducts { get; set; }
    }

}