using DigitalArena.DBContext;
using DigitalArena.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DigitalArena.Controllers
{
    //[Authorize(Roles = "ADMIN")]
    public class AdminDashboardController : Controller
    {

        private readonly DigitalArenaDBContext _dbContext = new DigitalArenaDBContext();

        [Route("admin/dashboard")]
        public ActionResult Index()
        {
            // Dummy data for dashboard stats
            ViewBag.TotalBuyers = 256;
            ViewBag.OrdersToday = 34;
            ViewBag.Revenue = 12540.75m;
            ViewBag.NewReviews = 12;

            // Dummy recent orders list
            ViewBag.RecentOrders = new List<dynamic>
            {
                new { OrderId = 101, BuyerName = "John Doe", TotalAmount = 150.00m, Status = "Completed", OrderDate = DateTime.Today.AddDays(-1) },
                new { OrderId = 102, BuyerName = "Jane Smith", TotalAmount = 249.99m, Status = "Pending", OrderDate = DateTime.Today.AddDays(-2) },
                new { OrderId = 103, BuyerName = "Alice Johnson", TotalAmount = 89.50m, Status = "Completed", OrderDate = DateTime.Today.AddDays(-3) },
                new { OrderId = 104, BuyerName = "Bob Brown", TotalAmount = 300.00m, Status = "Pending", OrderDate = DateTime.Today.AddDays(-4) },
            };

            return View();
        }


        [Route("admin/manage-buyers", Name = "ManageBuyersRoute")]
        public ActionResult ManageBuyers(int page = 1, int pageSize = 10, string search = "", string status = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbContext.User.Where(u => u.Role == "BUYER");

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.Username.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.FullName.Contains(search));
            }

            if (status == "active")
                query = query.Where(u => u.IsActive);
            else if (status == "inactive")
                query = query.Where(u => !u.IsActive);

            if (fromDate.HasValue)
                query = query.Where(u => u.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(u => u.CreatedAt <= toDate.Value);

            int totalBuyers = query.Count();

            var buyers = query
                .OrderBy(u => u.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    ProfileImage = u.ProfileImage,
                    FullName = u.FullName,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt ?? DateTime.MinValue
                })
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalBuyers / pageSize);

            if (Request.IsAjaxRequest())
                return PartialView("_BuyersTable", buyers);

            return View(buyers);
        }


        [Route("admin/manage-sellers", Name = "ManageSellersRoute")]
        public ActionResult ManageSellers(int page = 1, int pageSize = 10, string search = "", string status = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbContext.User.Where(u => u.Role == "SELLER");

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.Username.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.FullName.Contains(search));
            }

            if (status == "active")
                query = query.Where(u => u.IsActive);
            else if (status == "inactive")
                query = query.Where(u => !u.IsActive);

            if (fromDate.HasValue)
                query = query.Where(u => u.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(u => u.CreatedAt <= toDate.Value);

            int totalSellers = query.Count();

            var sellers = query
                .OrderBy(u => u.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    ProfileImage = u.ProfileImage,
                    FullName = u.FullName,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt ?? DateTime.MinValue
                })
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalSellers / pageSize);

            if (Request.IsAjaxRequest())
                return PartialView("_SellersTable", sellers);

            return View(sellers);
        }


        [Route("admin/manage-products", Name = "ManageProductsRoute")]
        public ActionResult ManageProducts(
        int page = 1,
        int pageSize = 10,
        string search = "",
        string sellerSearch = "",
        string status = "",
        int? categoryId = null)
        {
            var query = _dbContext.Product
                .Include(p => p.Category)
                .Include(p => p.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(sellerSearch))
            {
                query = query.Where(p => p.User.Username.Contains(sellerSearch));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            int totalProducts = query.Count();

            var products = query
                .OrderBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.Categories = _dbContext.Category.ToList();

            if (Request.IsAjaxRequest())
                return PartialView("_ProductsTable", products);

            return View(products);
        }




        [Route("admin/manage-reviews", Name = "ManageReviewsRoute")]
        public ActionResult ManageReviews(
        int page = 1,
        int pageSize = 10,
        string search = "",
        string status = "",
        DateTime? fromDate = null,
        DateTime? toDate = null)
        {
            var query = _dbContext.Review.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r =>
                    r.User.Username.Contains(search) ||
                    r.User.Email.Contains(search) ||
                    r.User.FullName.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (fromDate.HasValue)
                query = query.Where(r => r.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.CreatedAt <= toDate.Value);

            int totalReviews = query.Count();

            var reviews = query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalReviews / pageSize);

            if (Request.IsAjaxRequest())
                return PartialView("_ManageReviewsPartial", reviews);

            return View(reviews);
        }



        [Route("admin/profile", Name = "AdminProfileRoute")]
        public ActionResult AdminProfile()
        {
            return View();
        }
    }
}