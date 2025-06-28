using DigitalArena.DBContext;
using DigitalArena.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DigitalArena.Controllers.Admin
{
    [Authorize(Roles = "ADMIN")]
    public class ManageBuyerController : Controller
    {
        private readonly DigitalArenaDBContext _dbContext = new DigitalArenaDBContext();

        [Route("admin/manage-buyers/buyer-details", Name = "BuyerDetailsRoute")]
        public ActionResult BuyerDetails(int id)
        {
            var userEntity = _dbContext.User.FirstOrDefault(u => u.UserId == id && u.Role == "Buyer");

            if (userEntity == null)
            {
                return HttpNotFound();
            }

            var walletEntity = _dbContext.Wallet.FirstOrDefault(w => w.UserId == userEntity.UserId);

            var userModel = new UserModel
            {
                UserId = userEntity.UserId,
                Username = userEntity.Username,
                Email = userEntity.Email,
                Password = userEntity.Password,
                Phone = userEntity.Phone,
                Role = userEntity.Role,
                IsActive = userEntity.IsActive,
                ProfileImage = userEntity.ProfileImage,
                FullName = userEntity.FullName,
                CreatedAt = userEntity.CreatedAt,
                LastLoginAt = userEntity.LastLoginAt ?? DateTime.MinValue,
                Wallet = walletEntity != null
                    ? new WalletModel
                    {
                        WalletId = walletEntity.WalletId,
                        Balance = walletEntity.Balance,
                        Pin = walletEntity.Pin,
                        UserId = walletEntity.UserId
                    }
                    : null
            };

            return View(userModel);
        }







        [Route("admin/manage-buyers/buyer-details/{userId}/coupons", Name = "ViewCouponsRoute")]
        public ActionResult ViewCoupons(int userId)
        {
            // Validate buyer exists
            var user = _dbContext.User.FirstOrDefault(u => u.UserId == userId && u.Role == "Buyer");
            if (user == null)
                return HttpNotFound();

            // Fetch coupons for that user (user-specific and global coupons)
            var coupons = _dbContext.Coupon
                .Where(c => c.UserSpecific == true && c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            ViewBag.UserName = user.FullName ?? user.Username;
            ViewBag.UserId = userId;

            return View(coupons);
        }


        
        
        
        
        [Route("admin/manage-buyers/buyer-details/{userId}/orders", Name = "ViewOrdersRoute")]
        public ActionResult ViewOrders(int userId)
        {
            // Validate buyer exists
            var user = _dbContext.User.FirstOrDefault(u => u.UserId == userId && u.Role == "Buyer");
            if (user == null)
                return HttpNotFound();

            // Fetch orders for that user, ordered by CreatedAt descending
            var orders = _dbContext.Order
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderModel
                {
                    OrderId = o.OrderId,
                    Amount = o.Amount,
                    BillingAddress = o.BillingAddress,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status,
                    PaymentStatus = o.PaymentStatus,
                    TransactionId = o.TransactionId,
                    UserId = o.UserId
                })
                .ToList();

            ViewBag.UserName = user.FullName ?? user.Username;
            ViewBag.UserId = userId;

            return View(orders);
        }

        
        
        
        
        [Route("admin/manage-buyers/buyer-details/{userId}/orders/{orderId}/order-details", Name = "ViewOrderDetailsRoute")]
        public ActionResult ViewOrderDetails(int userId, int orderId)
        {
            // Validate buyer exists
            var user = _dbContext.User.FirstOrDefault(u => u.UserId == userId && u.Role == "Buyer");
            if (user == null)
                return HttpNotFound();

            // Validate order exists and belongs to user
            var order = _dbContext.Order.Where(o => o.OrderId == orderId && o.UserId == userId).FirstOrDefault();

            if (order == null)
                return HttpNotFound();

            // Eager load OrderItems with Product info
            var orderItems = _dbContext.OrderItem
                .Where(oi => oi.OrderId == orderId)
                .ToList();

            ViewBag.UserName = user.FullName ?? user.Username;
            ViewBag.UserId = userId;

            return View(orderItems);
        }





        [Route("admin/manage-buyers/buyer-details/{userId}/purchased-products", Name = "ViewPurchasedProductsRoute")]
        public ActionResult ViewPurchasedProducts(int userId)
        {
            // Check if user is a Buyer
            var user = _dbContext.User.FirstOrDefault(u => u.UserId == userId && u.Role == "Buyer");
            if (user == null)
                return HttpNotFound("Buyer not found");

            // Get purchased products (valid permissions)
            var purchasedProducts = _dbContext.Permission
                .Where(p => p.UserId == userId && p.IsValid)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            // ViewBag or ViewModel
            ViewBag.UserName = user.FullName ?? user.Username;
            ViewBag.UserId = userId;

            return View(purchasedProducts);
        }






        [Route("admin/manage-buyers/buyer-details/{userId}/transactions", Name = "ViewTransactionsRoute")]
        public ActionResult ViewTransactions(int userId)
        {
            // Validate buyer
            var user = _dbContext.User.FirstOrDefault(u => u.UserId == userId && u.Role == "Buyer");
            if (user == null)
                return HttpNotFound("Buyer not found");

            // Fetch transactions through Wallets
            var transactions = _dbContext.Transaction
                .Where(t => t.Wallet.UserId == userId)
                .OrderByDescending(t => t.TransactionId)
                .ToList();

            ViewBag.UserName = user.FullName ?? user.Username;
            ViewBag.UserId = userId;

            return View(transactions);
        }







        [HttpPost]
        public ActionResult UpdateBuyerStatus(int userId, bool newStatus)
        {
            var buyer = _dbContext.User.FirstOrDefault(u => u.UserId == userId && u.Role == "BUYER");
            if (buyer == null)
                return HttpNotFound();

            buyer.IsActive = newStatus;
            _dbContext.SaveChanges();

            return RedirectToAction("BuyerDetails", new { id = userId });
        }

    }
}