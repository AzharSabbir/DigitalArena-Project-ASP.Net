using DigitalArena.DBContext;
using DigitalArena.Models;
using DigitalArena.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DigitalArena.Controllers.Admin
{
    [Authorize(Roles = "ADMIN")]
    public class ManageSellerController : Controller
    {
        private readonly DigitalArenaDBContext _dbContext = new DigitalArenaDBContext();


        [Route("admin/manage-sellers/seller-details", Name = "SellerDetailsRoute")]
        public ActionResult SellerDetails(int id)
        {
            var seller = _dbContext.User.FirstOrDefault(u => u.UserId == id && u.Role == "Seller");
            if (seller == null)
                return HttpNotFound("Seller not found.");

            var productList = _dbContext.Product.Where(p => p.SellerId == id).ToList();
            var productIds = productList.Select(p => p.ProductId).ToList();

            // Order + Revenue Calculation
            var orderItems = _dbContext.OrderItem.Where(oi => productIds.Contains(oi.ProductId)).ToList();
            var paidOrderIds = _dbContext.Order.Where(o => o.PaymentStatus == "Paid").Select(o => o.OrderId).ToList();
            var sellerOrderItems = orderItems.Where(oi => paidOrderIds.Contains(oi.OrderId)).ToList();

            // Only Approved Reviews Calculation
            var sellerReviews = _dbContext.Review
                .Where(r => productIds.Contains(r.ProductId) && r.Status == "Approved")
                .ToList();

            int totalReviews = sellerReviews.Count;
            double avgRating = totalReviews > 0 ? sellerReviews.Average(r => r.Rating) : 0;


            // Compose ViewModel
            var sellerViewModel = new SellerDetailsViewModel
            {
                Seller = new UserModel
                {
                    UserId = seller.UserId,
                    Username = seller.Username,
                    Email = seller.Email,
                    Password = seller.Password,
                    Phone = seller.Phone,
                    Role = seller.Role,
                    IsActive = seller.IsActive,
                    ProfileImage = seller.ProfileImage,
                    FullName = seller.FullName,
                    CreatedAt = seller.CreatedAt,
                    LastLoginAt = seller.LastLoginAt ?? DateTime.MinValue,
                },
                ProductCount = productList.Count,
                TotalOrders = sellerOrderItems.Select(oi => oi.OrderId).Distinct().Count(),
                TotalRevenue = sellerOrderItems.Sum(oi => oi.Price),
                TotalReviews = totalReviews,
                AverageRating = Math.Round(avgRating, 2)
            };

            return View(sellerViewModel);
        }







        [Route("admin/manage-sellers/seller-details/{userId}/products", Name = "ViewSellerProductsRoute")]
        public ActionResult ViewSellerProducts(
            int userId,
            string search = "",
            string status = "",
            string isActive = "",
            string priceSort = "",
            decimal? priceMin = null,
            decimal? priceMax = null
        )
        {
            var seller = _dbContext.User.FirstOrDefault(u => u.UserId == userId && u.Role == "Seller");
            if (seller == null)
                return HttpNotFound("Seller not found.");

            var query = _dbContext.Product.Where(p => p.SellerId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.Category.Name.ToLower().Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrEmpty(isActive))
            {
                bool active = isActive == "true";
                query = query.Where(p => p.IsActive == active);
            }

            if (priceMin.HasValue)
                query = query.Where(p => p.Price >= priceMin.Value);

            if (priceMax.HasValue)
                query = query.Where(p => p.Price <= priceMax.Value);

            if (priceSort == "asc")
                query = query.OrderBy(p => p.Price);
            else if (priceSort == "desc")
                query = query.OrderByDescending(p => p.Price);
            else
                query = query.OrderBy(p => p.ProductId); // default order

            var products = query.ToList();

            ViewBag.SellerName = seller.FullName;
            ViewBag.SellerId = seller.UserId;

            if (Request.IsAjaxRequest())
                return PartialView("_SellerProductsTable", products);

            return View(products);
        }







        [Route("admin/manage-sellers/seller-details/{userId}/reviews", Name = "ViewSellerProductReviewsRoute")]
        public ActionResult ViewSellerProductReviews(
        int userId,
        int? rating = null,
        string status = null,
        int? productId = null,
        string productName = null,
        DateTime? fromDate = null,
        DateTime? toDate = null
        )
        {
            var query = _dbContext.Review.AsQueryable();

            query = query.Where(r => r.Product.SellerId == userId);

            if (rating.HasValue)
                query = query.Where(r => r.Rating == rating.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status == status);

            if (productId.HasValue)
                query = query.Where(r => r.ProductId == productId.Value);

            if (!string.IsNullOrWhiteSpace(productName))
                query = query.Where(r => r.Product.Name.Contains(productName));

            if (fromDate.HasValue)
                query = query.Where(r => r.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.CreatedAt <= toDate.Value);

            var reviews = query.OrderByDescending(r => r.CreatedAt).ToList();

            ViewBag.SellerId = userId;
            ViewBag.SellerName = _dbContext.User
                .Where(u => u.UserId == userId)
                .Select(u => u.Username)
                .FirstOrDefault();

            if (Request.IsAjaxRequest())
                return PartialView("_SellerReviewsTable", reviews);

            return View(reviews);
        }




        [HttpPost]
        public ActionResult UpdateSellerStatus(int userId, bool newStatus)
        {
            var seller = _dbContext.User.FirstOrDefault(u => u.UserId == userId && u.Role == "Seller");
            if (seller == null)
                return HttpNotFound();

            seller.IsActive = newStatus;
            _dbContext.SaveChanges();

            return RedirectToAction("SellerDetails", new { id = userId });
        }

    }
}